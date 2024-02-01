using Enviroment.Data;
using Enviroment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class CategoryController : Controller
{
    private readonly HelpdeskContext _context;

    // Initializes with database context.
    public CategoryController(HelpdeskContext context)
    {
        _context = context;
    }

    // Lists all categories.
    public async Task<IActionResult> Index()
    {
        // Fetches categories asynchronously.
        var categories = await _context.Categorys.ToListAsync();
        return View(categories);
    }

    // Shows create form.
    public IActionResult Create()
    {
        // Returns a blank form.
        return View();
    }

    // Processes category creation.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        // Validates form data.
        if (ModelState.IsValid)
        {
            // Adds new category.
            _context.Add(category);
            // Saves changes asynchronously.
            await _context.SaveChangesAsync();
            // Redirects to listing.
            return RedirectToAction(nameof(Index));
        }
        // Returns form with errors.
        return View(category);
    }

    // Shows edit form.
    public async Task<IActionResult> Edit(int? id)
    {
        // Checks for null ID.
        if (id == null)
        {
            return NotFound();
        }

        // Finds category asynchronously.
        var category = await _context.Categorys.FindAsync(id);
        // Handles non-existent category.
        if (category == null)
        {
            return NotFound();
        }
        // Returns edit form.
        return View(category);
    }

    // Updates category details.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Category_id,Case_Name,Description")] Category category)
    {
        // Checks ID match.
        if (id != category.Category_id)
        {
            return NotFound();
        }

        // Validates model state.
        if (ModelState.IsValid)
        {
            try
            {
                // Applies changes.
                _context.Update(category);
                // Saves changes asynchronously.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifies category exists.
                if (!CategoryExists(category.Category_id))
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
        return View(category);
    }

    // Shows delete confirmation.
    public async Task<IActionResult> Delete(int? id)
    {
        // Checks for null ID.
        if (id == null)
        {
            return NotFound();
        }

        // Finds category for deletion.
        var category = await _context.Categorys
            .FirstOrDefaultAsync(m => m.Category_id == id);
        // Handles non-existent category.
        if (category == null)
        {
            return NotFound();
        }
        // Returns delete view.
        return View(category);
    }

    // Confirms category deletion.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Finds category asynchronously.
        var category = await _context.Categorys.FindAsync(id);
        // Handles null category.
        if (category == null)
        {
            return NotFound();
        }
        // Removes category.
        _context.Categorys.Remove(category);
        // Saves changes asynchronously.
        await _context.SaveChangesAsync();
        // Redirects to listing.
        return RedirectToAction(nameof(Index));
    }

    // Checks category existence.
    private bool CategoryExists(int id)
    {
        // Returns existence status.
        return _context.Categorys.Any(e => e.Category_id == id);
    }
}
