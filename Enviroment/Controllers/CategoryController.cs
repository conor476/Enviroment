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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categorys.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Category_id,Case_Name,Description,Group,About")] Category category)
    {
        if (id != category.Category_id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Category_id))
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
        return View(category);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categorys
                                     .FirstOrDefaultAsync(m => m.Category_id == id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categorys.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        _context.Categorys.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CategoryExists(int id)
    {
        return _context.Categorys.Any(e => e.Category_id == id);
    }
}
