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

    // Storing the current filter parameters in the ViewBag to use them in the view
    ViewBag.CurrentFilter = searchString;
    ViewBag.CurrentStatus = status;
    ViewBag.SortDescending = sortDescending;
    ViewBag.TypeFilter = typeFilter;
    ViewBag.IsNewChecked = isNewChecked;

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
        public async Task<IActionResult> Create([Bind("EmployeeName,Description,Category,Status,Team,Summary,Type,EmailAddress,CustomerName, PriorityLevel")] Ticket ticket)
        {
            if (User.IsInRole("Admin"))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == ticket.EmailAddress);
             
            }
            else
            {
               
                ticket.EmailAddress = User.Identity.Name;
            }

            if (ModelState.IsValid)
            {
                ticket.DateCreated = DateTime.Now; // Record the date and time when the ticket is created
        ticket.Status = "New"; // Set the initial status to "New"
        ticket.PriorityLevel = ticket.PriorityLevel;
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
            // Check if the id parameter is null. If so, return a 404 Not Found response.
            if (id == null)
            {
                return NotFound();
            }

            // Find the ticket by id from the database. If not found, return a 404 Not Found response.
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Retrieve categories from the database to populate the dropdown list in the view.
            var categories = await _context.Categorys
                                           .Select(c => new { c.Case_Name, c.Description })
                                           .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Case_Name", "Case_Name");
            ViewBag.CategoryDescriptions = categories.ToDictionary(c => c.Case_Name, c => c.Description);

            // Return the Edit view, passing in the ticket to be edited.
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketID,CustomerName,EmployeeName,EmailAddress,Description,Category,Status,PriorityLevel,Team,Summary,Type,NewNote")] Ticket ticket)
        {
            // Retrieve the existing ticket from the database. If not found, return a 404 Not Found response.
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == id);
            if (existingTicket == null)
            {
                return NotFound();
            }

            // Check if a new note is added and if so, append it to the existing ticket description.
            if (!string.IsNullOrEmpty(ticket.NewNote))
            {
                string userName = User.Identity.Name; // Get the current user's name.
                existingTicket.Description += $"\n[Note added by {userName} on {DateTime.Now}]: {ticket.NewNote}";
                existingTicket.LastUpdated = DateTime.Now; // Update the LastUpdated field.

               

                // Check if the user is an Admin and an email address is provided, then send an email notification
                if (User.IsInRole("Admin") && !string.IsNullOrEmpty(ticket.EmailAddress))
                {
                    // Construct the subject line with ticket type and ticket number
                    string emailSubject = $"New Note Added to Your Ticket - {existingTicket.Type} #{existingTicket.TicketID}";

                    // Send the email with the customized subject line
                    await _emailService.SendEmailAsync(ticket.EmailAddress, emailSubject, ticket.NewNote, existingTicket.TicketID, existingTicket.Type);
                }

            }

            // Validate the model. If valid, proceed to update the ticket.
            if (ModelState.IsValid)
            {
                // Update the OpenedDate and ClosedDate based on the status changes.
                if (existingTicket.Status != "Open" && ticket.Status == "Open" && existingTicket.OpenedDate == null)
                {
                    existingTicket.OpenedDate = DateTime.Now;
                }

                if (existingTicket.Status != "Closed" && ticket.Status == "Closed")
                {
                    existingTicket.ClosedDate = DateTime.Now;
                }

                // Update the ticket properties with the values from the form.
                existingTicket.Status = ticket.Status;
                existingTicket.Category = ticket.Category;
                existingTicket.Team = ticket.Team;
                existingTicket.Summary = ticket.Summary;
                existingTicket.Type = ticket.Type;
                existingTicket.CustomerName = ticket.CustomerName;
                existingTicket.EmployeeName = ticket.EmployeeName;
                existingTicket.EmailAddress = ticket.EmailAddress;
                existingTicket.PriorityLevel = ticket.PriorityLevel;

                // Save the updated ticket to the database.
                _context.Update(existingTicket);
                await _context.SaveChangesAsync();

                // Redirect to the Index action after successful update.
                return RedirectToAction(nameof(Index));
            }

            // If the model is not valid, reload the categories and return to the Edit view with the current ticket.
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
