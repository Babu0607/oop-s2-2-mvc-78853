using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using oop_s2_2_mvc_78853.Models;
using oop_s2_2_mvc_78853.Data;
using Serilog;

namespace oop_s2_2_mvc_78853.Controllers;

[Authorize(Roles = "Admin,Inspector,Viewer")]
public class FollowUpsController: Controller
{
    private readonly ApplicationDbContext _context;

    public FollowUpsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var followUps = await _context.FollowUps
            .Include(f => f.Inspection)
            .ThenInclude(i => i.Premises)
            .ToListAsync();
    
        ViewBag.TotalCount = followUps.Count;
    
        return View(followUps);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var followUp = await _context.FollowUps
            .Include(f => f.Inspection)
            .ThenInclude(i => i.Premises)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (followUp == null) return NotFound();

        return View(followUp);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create()
    {
        ViewData["InspectionId"] = new SelectList(await _context.Inspections
            .Include(i => i.Premises)
            .ToListAsync(), "Id", "Premises.Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(FollowUp followUp)
    {
        var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
        if (inspection != null && followUp.DueDate < inspection.InspectionDate)
        {
            ModelState.AddModelError("DueDate", "Due date cannot be before inspection date");
            Log.Warning("FollowUp creation attempted with due date before inspection date: Inspection {InspectionId}", 
                followUp.InspectionId);
        }

        if (ModelState.IsValid)
        {
            _context.Add(followUp);
            await _context.SaveChangesAsync();
            
            Log.Information("FollowUp created: {FollowUpId} for Inspection {InspectionId}", 
                followUp.Id, followUp.InspectionId);
            
            return RedirectToAction(nameof(Index));
        }
        ViewData["InspectionId"] = new SelectList(await _context.Inspections.ToListAsync(), "Id", "Id", followUp.InspectionId);
        return View(followUp);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var followUp = await _context.FollowUps.FindAsync(id);
        if (followUp == null) return NotFound();
        
        ViewData["InspectionId"] = new SelectList(await _context.Inspections.ToListAsync(), "Id", "Id", followUp.InspectionId);
        return View(followUp);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int id, FollowUp followUp)
    {
        if (id != followUp.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(followUp);
                await _context.SaveChangesAsync();
                
                Log.Information("FollowUp updated: {FollowUpId}", followUp.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FollowUpExists(followUp.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["InspectionId"] = new SelectList(await _context.Inspections.ToListAsync(), "Id", "Id", followUp.InspectionId);
        return View(followUp);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var followUp = await _context.FollowUps
            .Include(f => f.Inspection)
            .ThenInclude(i => i.Premises)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (followUp == null) return NotFound();

        return View(followUp);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var followUp = await _context.FollowUps.FindAsync(id);
        if (followUp != null)
        {
            _context.FollowUps.Remove(followUp);
            await _context.SaveChangesAsync();
            
            Log.Information("FollowUp deleted: {FollowUpId}", id);
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Close(int id)
    {
        var followUp = await _context.FollowUps.FindAsync(id);
        if (followUp == null) return NotFound();

        if (followUp.Status == "Open")
        {
            followUp.Status = "Closed";
            followUp.ClosedDate = DateTime.Today;
            await _context.SaveChangesAsync();
            
            Log.Information("FollowUp closed: {FollowUpId}", id);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool FollowUpExists(int id)
    {
        return _context.FollowUps.Any(e => id == e.Id);
    }
}