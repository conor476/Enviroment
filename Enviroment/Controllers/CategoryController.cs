using Enviroment.Data;
using Enviroment.Models;
using Enviroment.ViewModels; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CategoryController : Controller
{
    private readonly HelpdeskContext _context;

    public CategoryController(HelpdeskContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Fetches categories and groups them by 'Group'.
        var categories = await _context.Categorys
                                       .OrderBy(c => c.Group)
                                       .ThenBy(c => c.Case_Name)
                                       .ToListAsync();

        // Convert to List of GroupedCategory instead of Dictionary
        var groupedCategories = categories.GroupBy(c => c.Group)
                                          .Select(group => new GroupedCategory
                                          {
                                              GroupName = group.Key,
                                              Categories = group.ToList()
                                          }).ToList();

        return View(groupedCategories);
    }



    public IActionResult Create()
    {
        return View();
    }

    // POST: Create a new category
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        // Check if the model state is valid
        if (ModelState.IsValid)
        {
            // Add the new category to the context
            _context.Add(category);
            // Save the changes asynchronously to the database
            await _context.SaveChangesAsync();
            // Redirect to the Index view on successful creation
            return RedirectToAction(nameof(Index));
        }
        // If the model state is not valid, return the same view for correction
        return View(category);
    }

    // GET: Edit an existing category by its id
    public async Task<IActionResult> Edit(int? id)
    {
        // Check if the provided id is null. If so, return a 404 Not Found response.
        if (id == null)
        {
            return NotFound();
        }

        // Find the category by its id. If not found, return a 404 Not Found response.
        var category = await _context.Categorys.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        // Return the Edit view, passing the category to be edited.
        return View(category);
    }

    // POST: Edit an existing category
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Category_id,Case_Name,Description,Group,About")] Category category)
    {
        // Check if the category's id matches the id in the route
        if (id != category.Category_id)
        {
            return NotFound(); // return a 404 Not Found response.
        }

        // Check if the model state is valid
        if (ModelState.IsValid)
        {
            try
            {
                // Update the category in the context
                _context.Update(category);
                // Save the changes asynchronously to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // If there's a concurrency error, check if the category still exists
                if (!CategoryExists(category.Category_id))
                {
                    return NotFound(); // If not, return a 404 Not Found response.
                }
                else
                {
                    throw; // Re-throw the exception if the category exists
                }
            }
            // Redirect to the Index view on successful update
            return RedirectToAction(nameof(Index));
        }
        // If the model state is not valid, return the same view for correction
        return View(category);
    }

    // GET: Delete a category by its id
    public async Task<IActionResult> Delete(int? id)
    {
        // Check if the provided id is null. If so, return a 404 Not Found response.
        if (id == null)
        {
            return NotFound();
        }

        // Find the category by its id.  return a 404 Not Found response.
        var category = await _context.Categorys
                                     .FirstOrDefaultAsync(m => m.Category_id == id);
        if (category == null)
        {
            return NotFound();
        }
        // Return the Delete view, passing the category to be deleted.
        return View(category);
    }

    // POST: Confirm the deletion of a category
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Find the category by its id
        var category = await _context.Categorys.FindAsync(id);
        if (category == null)
        {
            return NotFound(); //  return a 404 Not Found response.
        }
        // Remove the category from the context
        _context.Categorys.Remove(category);
        // Save the changes asynchronously to the database
        await _context.SaveChangesAsync();
        // Redirect to the Index view on successful deletion
        return RedirectToAction(nameof(Index));
    }

    // Check if a category exists by its id
    private bool CategoryExists(int id)
    {
        // Return true if any category in the context matches the given id
        return _context.Categorys.Any(e => e.Category_id == id);
    }
}