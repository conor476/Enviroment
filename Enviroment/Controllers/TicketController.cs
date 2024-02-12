
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Enviroment.Data;
using Enviroment.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Environment.Controllers;
public class TicketController : Controller

{
    private readonly HelpdeskContext _context;

    // Inject UserManager into your controller
    private readonly UserManager<User> _userManager;

    public TicketController(HelpdeskContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<IActionResult> Index(string searchString)
    {
        var ticketsQuery = _context.Tickets.AsQueryable();

        // Check if the user is not an admin
        if (!User.IsInRole("Admin"))
        {
            // Get the current user's email address
            string userEmail = User.Identity.Name; // Adjust this if needed

            // Filter tickets to show only those associated with the logged-in user's email
            ticketsQuery = ticketsQuery.Where(t => t.EmailAddress == userEmail);
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            ticketsQuery = ticketsQuery.Where(t => t.TicketID.ToString().Contains(searchString)
                                                   || t.CustomerName.Contains(searchString)
                                                   // Include other fields if necessary for the search.
                                                   );
        }

        var tickets = await ticketsQuery.ToListAsync();

        // Redirect to the edit page if only one ticket is found
        if (tickets.Count == 1)
        {
            return RedirectToAction("Edit", new { id = tickets.Single().TicketID });
        }

        return View(tickets);
    }

    // Displays open tickets.
    public async Task<IActionResult> OpenTickets()
    {
        var openTickets = await _context.Tickets
                                        .Where(t => t.Status == "Open")
                                        .ToListAsync();
        return View("Index", openTickets);
    }

    // Shows form for creating a ticket.
    public async Task<IActionResult> Create()
    {
        var categories = await _context.Categorys
                                       .Select(c => new { c.Case_Name, c.Description })
                                       .ToListAsync();

        ViewBag.Categories = new SelectList(categories, "Case_Name", "Case_Name");
        ViewBag.CategoryDescriptions = categories.ToDictionary(c => c.Case_Name, c => c.Description);

        ViewBag.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier); // Set the user ID
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EmployeeName,Description,Category,Status,Team,Summary,Type")] Ticket ticket, string userEmail = "")
    {
        // If an admin is logged in and an email is provided
        if (User.IsInRole("Admin") && !string.IsNullOrEmpty(userEmail))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user != null)
            {
                ticket.CustomerName = user.Id; // Assuming 'Id' is the user ID in your User model
                ticket.EmailAddress = user.Email; // Set the email address from the user found by email
            }
        }
        else // For non-admin users, automatically set their own ID and email
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ticket.CustomerName = userId;
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                ticket.EmailAddress = user.Email; // Set the email address from the logged-in user
            }
        }

        if (ModelState.IsValid)
        {
            _context.Add(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { id = ticket.TicketID });
        }

        // Logic to handle form re-display in case of invalid state
        var categories = await _context.Categorys
                                       .Select(c => new { c.Case_Name, c.Description })
                                       .ToListAsync();
        ViewBag.Categories = new SelectList(categories, "Case_Name", "Case_Name");
        ViewBag.CategoryDescriptions = categories.ToDictionary(c => c.Case_Name, c => c.Description);

        return View(ticket);
    }

    // Shows form for editing a ticket.
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }
        return View(ticket);
    }

    // Processes ticket editing with new note functionality.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("TicketID,CustomerName,EmployeeName,EmailAddress,Description,Category,Status,Team,Summary,Type,NewNote")] Ticket ticket)
    {
        if (id != ticket.TicketID)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(ticket.NewNote))
        {
            // Get the current user's name
            string userName = User.Identity.Name;

            // Append the new note with a timestamp and the user's name.
            ticket.Description += $"\n[Note added by {userName} on {DateTime.Now}]: {ticket.NewNote}";
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tickets.Any(e => e.TicketID == ticket.TicketID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(ticket);
    }

    // Shows confirmation for deleting a ticket.
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ticket = await _context.Tickets
                                   .FirstOrDefaultAsync(m => m.TicketID == id);
        if (ticket == null)
        {
            return NotFound();
        }
        return View(ticket);
    }

    // Processes ticket deletion.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

