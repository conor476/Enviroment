using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Enviroment.Data;
using Enviroment.Models;
using X.PagedList;
namespace Enviroment.Controllers
{
    public class TicketController : Controller
    {
        private readonly HelpdeskContext _context;
        private readonly UserManager<User> _userManager;
        private readonly EmailService _emailService;

        public TicketController(HelpdeskContext context, UserManager<User> userManager, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10; // Set your desired page size

            var ticketsQuery = _context.Tickets.AsQueryable();

            // Restrict tickets to the current user's email if not an admin
            if (!User.IsInRole("Admin"))
            {
                string userEmail = User.Identity.Name;
                ticketsQuery = ticketsQuery.Where(t => t.EmailAddress == userEmail);
            }

            // Apply search filter if a searchString is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                ticketsQuery = ticketsQuery.Where(t => t.TicketID.ToString().Contains(searchString)
                                                       || t.CustomerName.Contains(searchString));
            }

            // Sorting tickets by LastUpdated in descending order
            var pagedTickets = await ticketsQuery.OrderBy(t => t.LastUpdated) // Changed line for sorting
                                                 .ToPagedListAsync(pageNumber, pageSize);

            return View(pagedTickets);
        }



        public async Task<IActionResult> OpenTickets(int? page)
        {
            int pageSize = 10; // Set the number of items per page
            int pageNumber = (page ?? 1);

            var openTicketsQuery = _context.Tickets.Where(t => t.Status == "Open");
            var openTickets = await openTicketsQuery.ToPagedListAsync(pageNumber, pageSize);

            return View("Index", openTickets);
        }
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categorys
                                           .Select(c => new { c.Case_Name, c.Description })
                                           .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "Case_Name", "Case_Name");
            ViewBag.CategoryDescriptions = categories.ToDictionary(c => c.Case_Name, c => c.Description);

            ViewBag.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeName,Description,Category,Status,Team,Summary,Type,EmailAddress,CustomerName")] Ticket ticket)
        {
            if (User.IsInRole("Admin"))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == ticket.EmailAddress);
                if (user != null)
                {
                    ticket.CustomerName = user.Id;
                }
            }
            else
            {
                ticket.CustomerName = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ticket.EmailAddress = User.Identity.Name;
            }

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = ticket.TicketID });
            }

            var categories = await _context.Categorys
                                           .Select(c => new { c.Case_Name, c.Description })
                                           .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Case_Name", "Case_Name");
            ViewBag.CategoryDescriptions = categories.ToDictionary(c => c.Case_Name, c => c.Description);

            return View(ticket);
        }

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
                string userName = User.Identity.Name;
                ticket.Description += $"\n[Note added by {userName} on {DateTime.Now}]: {ticket.NewNote}";

                // Update the LastUpdated property
                ticket.LastUpdated = DateTime.Now;

                // Send an email with the note content only if the user is an admin
                if (User.IsInRole("Admin"))
                {
                    await _emailService.SendEmailAsync(ticket.EmailAddress, "New Note Added to Your Ticket", ticket.NewNote);
                }
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


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FirstOrDefaultAsync(m => m.TicketID == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

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
}

