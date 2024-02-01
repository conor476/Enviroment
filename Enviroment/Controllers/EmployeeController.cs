using Enviroment.Data;
using Enviroment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Enviroment.Controllers;

// Handles Employee web requests.
public class EmployeeController : Controller
{
    // Provides database access.
    private readonly HelpdeskContext _context;

    // Initializes with database context.
    public EmployeeController(HelpdeskContext context)
    {
        _context = context;
    }

    // Displays all employees.
    public async Task<IActionResult> Index()
    {
        // Fetches employees asynchronously.
        var employees = await _context.Employees.ToListAsync();
        return View(employees);
    }

    // Shows create form.
    public IActionResult Create()
    {
        // Returns a blank form.
        return View();
    }

    // Processes employee creation.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        // Validates form data.
        if (ModelState.IsValid)
        {
            // Adds new employee.
            _context.Add(employee);
            // Saves changes asynchronously.
            await _context.SaveChangesAsync();
            // Redirects to listing.
            return RedirectToAction(nameof(Index));
        }
        // Returns form with errors.
        return View(employee);
    }

    // Shows edit form.
    public async Task<IActionResult> Edit(int? id)
    {
        // Checks for null ID.
        if (id == null)
        {
            return NotFound();
        }

        // Finds employee asynchronously.
        var employee = await _context.Employees.FindAsync(id);
        // Handles non-existent employee.
        if (employee == null)
        {
            return NotFound();
        }
        // Returns edit form.
        return View(employee);
    }

    // Updates employee details.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("EmployeeID,FirstName,LastName,Email,Password")] Employee employee)
    {
        // Checks ID match.
        if (id != employee.EmployeeID)
        {
            return NotFound();
        }

        // Validates model state.
        if (ModelState.IsValid)
        {
            try
            {
                // Applies changes.
                _context.Update(employee);
                // Saves changes asynchronously.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifies employee exists.
                if (!_context.Employees.Any(e => e.EmployeeID == employee.EmployeeID))
                {
                    return NotFound();
                }
                else
                {
                    // Rethrows other exceptions.
                    throw;
                }
            }
            // Redirects to listing.
            return RedirectToAction(nameof(Index));
        }
        // Returns form with errors.
        return View(employee);
    }

    // Shows delete confirmation.
    public async Task<IActionResult> Delete(int? id)
    {
        // Checks for null ID.
        if (id == null)
        {
            return NotFound();
        }

        // Finds employee asynchronously.
        var employee = await _context.Employees.FindAsync(id);
        // Handles non-existent employee.
        if (employee == null)
        {
            return NotFound();
        }
        // Returns delete view.
        return View(employee);
    }

    // Confirms employee deletion.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Finds employee asynchronously.
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            // Removes employee.
            _context.Employees.Remove(employee);
            // Saves changes asynchronously.
            await _context.SaveChangesAsync();
        }
        // Redirects to listing.
        return RedirectToAction(nameof(Index));
    }
}
