using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using oop_s2_2_mvc_78853.Models;
using oop_s2_2_mvc_78853.Data;
using Serilog;

namespace oop_s2_2_mvc_78853.Controllers;

public class InspectionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public InspectionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var inspections = await _context.Inspections
            .Include(i => i.Premises)
            .Include(i => i.FollowUps)
            .ToListAsync();
        return View(inspections);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var inspection = await _context.Inspections
            .Include(i => i.Premises)
            .Include(i => i.FollowUps)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (inspection == null) return NotFound();

        return View(inspection);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["PremisesId"] = new SelectList(await _context.Premises.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inspection inspection)
    {
        if (ModelState.IsValid)
        {
            _context.Add(inspection);
            await _context.SaveChangesAsync();
            
            Log.Information("Inspection created: {InspectionId} for Premises {PremisesId}", 
                inspection.Id, inspection.PremisesId);
            
            return RedirectToAction(nameof(Index));
        }
        ViewData["PremisesId"] = new SelectList(await _context.Premises.ToListAsync(), "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var inspection = await _context.Inspections.FindAsync(id);
        if (inspection == null) return NotFound();
        
        ViewData["PremisesId"] = new SelectList(await _context.Premises.ToListAsync(), "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Inspection inspection)
    {
        if (id != inspection.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(inspection);
                await _context.SaveChangesAsync();
                
                Log.Information("Inspection updated: {InspectionId}", inspection.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InspectionExists(inspection.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["PremisesId"] = new SelectList(await _context.Premises.ToListAsync(), "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var inspection = await _context.Inspections
            .Include(i => i.Premises)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (inspection == null) return NotFound();

        return View(inspection);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var inspection = await _context.Inspections.FindAsync(id);
        if (inspection != null)
        {
            _context.Inspections.Remove(inspection);
            await _context.SaveChangesAsync();
            
            Log.Information("Inspection deleted: {InspectionId}", id);
        }
        
        return RedirectToAction(nameof(Index));
    }

    private bool InspectionExists(int id)
    {
        return _context.Inspections.Any(e => e.Id == id);
    }
}