using Enviroment.Data;
using Enviroment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Enviroment.Controllers;
public class CustomerController : Controller
{
    private readonly HelpdeskContext _context;

    // Initializes with database context.
    public CustomerController(HelpdeskContext context)
    {
        _context = context;
    }

    // Lists all Customers.
    public async Task<IActionResult> Index()
    {
        // Fetches customers asynchronously.
        var customers = await _context.Customers.ToListAsync();
        return View(customers);
    }

    // Shows create form.
    public IActionResult Create()
    {
        // Returns a blank form.
        return View();
    }

    // Processes customer creation.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CustomerID,TicketNumber,FirstName,LastName,Email")] Customer customer)
    {
        // Validates form data.
        if (ModelState.IsValid)
        {
            // Adds new customer.
            _context.Add(customer);
            // Saves changes asynchronously.
            await _context.SaveChangesAsync();
            // Redirects to listing.
            return RedirectToAction(nameof(Index));
        }
        // Returns form with errors.
        return View(customer);
    }

    // Shows edit form.
    public async Task<IActionResult> Edit(int? id)
    {
        // Checks for null ID.
        if (id == null)
        {
            return NotFound();
        }

        // Finds customer asynchronously.
        var customer = await _context.Customers.FindAsync(id);
        // Handles non-existent customer.
        if (customer == null)
        {
            return NotFound();
        }
        // Returns edit form.
        return View(customer);
    }

    // Updates customer details.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CustomerID,TicketNumber,FirstName,LastName,Email")] Customer customer)
    {
        // Checks ID match.
        if (id != customer.CustomerID)
        {
            return NotFound();
        }

        // Validates model state.
        if (ModelState.IsValid)
        {
            try
            {
                // Applies changes.
                _context.Update(customer);
                // Saves changes asynchronously.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifies customer exists.
                if (!_context.Customers.Any(e => e.CustomerID == customer.CustomerID))
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
        return View(customer);
    }

    // Shows delete confirmation.
    public async Task<IActionResult> Delete(int? id)
    {
        // Checks for null ID.
        if (id == null)
        {
            return NotFound();
        }

        // Finds customer for deletion.
        var customer = await _context.Customers
            .FirstOrDefaultAsync(m => m.CustomerID == id);
        // Handles non-existent customer.
        if (customer == null)
        {
            return NotFound();
        }
        // Returns delete view.
        return View(customer);
    }

    // Confirms customer deletion.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Finds customer asynchronously.
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            // Removes customer.
            _context.Customers.Remove(customer);
            // Saves changes asynchronously.
            await _context.SaveChangesAsync();
        }
        // Redirects to listing.
        return RedirectToAction(nameof(Index));
    }
}
