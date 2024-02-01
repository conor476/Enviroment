using Enviroment.Data;
using Enviroment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Enviroment.Controllers;
public class TicketController : Controller
{
    private readonly HelpdeskContext _context;

    public TicketController(HelpdeskContext context)
    {
        _context = context;
    }

    // Displays all tickets or filters by search string, and redirects to edit page if one ticket is found.
    public async Task<IActionResult> Index(string searchString)
    {
        var ticketsQuery = _context.Tickets.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            ticketsQuery = ticketsQuery.Where(t => t.TicketID.ToString().Contains(searchString)
                                                   || t.CustomerName.Contains(searchString)
                                                   // Include other fields if necessary for the search.
                                                   );
        }

        var tickets = await ticketsQuery.ToListAsync();

        // Redirect to the edit page if only one ticket is found.
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

        return View();
    }

    // Processes ticket creation.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ticket ticket)
    {
        if (ModelState.IsValid)
        {
            _context.Add(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

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
    public async Task<IActionResult> Edit(int id, [Bind("TicketID,CustomerName,EmployeeName,Description,Category,Status,Team,NewNote")] Ticket ticket)
    {
        if (id != ticket.TicketID)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(ticket.NewNote))
        {
            // Append the new note with a timestamp.
            ticket.Description += $"\n[Note added on {DateTime.Now}]: {ticket.NewNote}";
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
