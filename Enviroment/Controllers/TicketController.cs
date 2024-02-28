using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Enviroment.Data;
using Enviroment.Models;
using Enviroment.Services;
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

        public async Task<IActionResult> Index(string searchString, string status, int? page, bool sortDescending = false, string typeFilter = null, bool isNewChecked = false)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

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

            // Apply case status filter if provided
            if (!string.IsNullOrEmpty(status))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Status == status);
            }

            // Filtering by Type (Service Request or Incident)
            if (!string.IsNullOrEmpty(typeFilter))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Type == typeFilter);
            }

            // Filtering by New Status
            if (isNewChecked)
            {
                ticketsQuery = ticketsQuery.Where(t => t.Status == "New");
            }

            // Sorting tickets by LastUpdated
            ticketsQuery = sortDescending ? ticketsQuery.OrderByDescending(t => t.LastUpdated) : ticketsQuery.OrderBy(t => t.LastUpdated);

            var pagedTickets = await ticketsQuery.ToPagedListAsync(pageNumber, pageSize);

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatus = status; // Persist the current status in the view
            ViewBag.SortDescending = sortDescending; // Persist the current sort order
            ViewBag.TypeFilter = typeFilter; // Persist the current type filter
            ViewBag.IsNewChecked = isNewChecked; // Persist the checkbox status

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
        ticket.DateCreated = DateTime.Now; // Record the date and time when the ticket is created
        ticket.Status = "New"; // Set the initial status to "New"

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

            // Load categories from the database for the dropdown
            var categories = await _context.Categorys
                                           .Select(c => new { c.Case_Name, c.Description })
                                           .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "Case_Name", "Case_Name");
            ViewBag.CategoryDescriptions = categories.ToDictionary(c => c.Case_Name, c => c.Description);

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketID,CustomerName,EmployeeName,EmailAddress,Description,Category,Status,Team,Summary,Type,NewNote")] Ticket ticket)
        {
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);
            if (existingTicket == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(ticket.NewNote))
            {
                string userName = User.Identity.Name;
                existingTicket.Description += $"\n[Note added by {userName} on {DateTime.Now}]: {ticket.NewNote}";
                existingTicket.LastUpdated = DateTime.Now;

                if (User.IsInRole("Admin") && !string.IsNullOrEmpty(ticket.EmailAddress))
                {
                    await _emailService.SendEmailAsync(ticket.EmailAddress, "New Note Added to Your Ticket", ticket.NewNote);
                }
            }

            if (ModelState.IsValid)
            {
                if (existingTicket.Status != "Open" && ticket.Status == "Open" && existingTicket.OpenedDate == null)
                {
                    existingTicket.OpenedDate = DateTime.Now;
                }

                if (existingTicket.Status != "Closed" && ticket.Status == "Closed")
                {
                    existingTicket.ClosedDate = DateTime.Now;
                }

                existingTicket.Status = ticket.Status;
                existingTicket.Category = ticket.Category;
                existingTicket.Team = ticket.Team;
                existingTicket.Summary = ticket.Summary;
                existingTicket.Type = ticket.Type;
                existingTicket.CustomerName = ticket.CustomerName;
                existingTicket.EmployeeName = ticket.EmployeeName;
                existingTicket.EmailAddress = ticket.EmailAddress;

                _context.Update(existingTicket);
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



        public async Task<IActionResult> KPIs()
        {
            var categories = await _context.Categorys.ToListAsync();
            var kpiData = new List<CategoryKPI>();

            foreach (var category in categories)
            {
                var tickets = await _context.Tickets
                                            .Where(t => t.Category == category.Case_Name)
                                            .ToListAsync();

                // Calculate Average Solve Time KPI
                var solveTimes = tickets.Where(t => t.OpenedDate != null && t.ClosedDate != null)
                                        .Select(t => (t.ClosedDate.Value - t.OpenedDate.Value).TotalHours);
                var averageSolveTime = solveTimes.Any() ? solveTimes.Average() : (double?)null;

                // Calculate First Response Time KPI
                var firstResponseTimes = tickets
                    .Where(t => t.DateCreated != null && t.OpenedDate != null)
                    .Select(t => (t.OpenedDate.Value - t.DateCreated.Value).TotalMinutes);

                var averageFirstResponseTime = firstResponseTimes.Any() ? firstResponseTimes.Average() : 0;
                // Calculate Ticket Volume KPI
                var ticketVolume = tickets.Count;

                // Calculate Ticket Backlog KPI
                var backlogTickets = tickets.Count(t => t.ClosedDate == null);

                // Calculate Incident vs Service Request KPI
                var incidentCount = tickets.Count(t => t.Type == "Incident");
                var serviceRequestCount = tickets.Count(t => t.Type == "Service Request");

                kpiData.Add(new CategoryKPI
                {
                    CategoryName = category.Case_Name,
                    AverageSolveTime = averageSolveTime,
                    AverageFirstResponseTime = averageFirstResponseTime,
                    TicketVolume = ticketVolume,
                    TicketBacklog = backlogTickets,
                    IncidentCount = incidentCount,                
                    ServiceRequestCount = serviceRequestCount
                });
            }

            return View(kpiData);
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
