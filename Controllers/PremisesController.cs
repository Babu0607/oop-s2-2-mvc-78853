using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_78853.Models;
using oop_s2_2_mvc_78853.Data;
using Serilog;

namespace oop_s2_2_mvc_78853.Controllers;

public class PremisesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PremisesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var premises = await _context.Premises.ToListAsync();
        return View(premises);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var premises = await _context.Premises
            .Include(p => p.Inspections)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (premises == null) return NotFound();

        return View(premises);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Premises premises)
    {
        if (ModelState.IsValid)
        {
            _context.Add(premises);
            await _context.SaveChangesAsync();
            
            Log.Information("Premises created: {PremisesId} - {PremisesName}", premises.Id, premises.Name);
            
            return RedirectToAction(nameof(Index));
        }
        return View(premises);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var premises = await _context.Premises.FindAsync(id);
        if (premises == null) return NotFound();
        
        return View(premises);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Premises premises)
    {
        if (id != premises.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(premises);
                await _context.SaveChangesAsync();
                
                Log.Information("Premises updated: {PremisesId} - {PremisesName}", premises.Id, premises.Name);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PremisesExists(premises.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(premises);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var premises = await _context.Premises
            .FirstOrDefaultAsync(m => m.Id == id);
        if (premises == null) return NotFound();

        return View(premises);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var premises = await _context.Premises.FindAsync(id);
        if (premises != null)
        {
            _context.Premises.Remove(premises);
            await _context.SaveChangesAsync();
            
            Log.Information("Premises deleted: {PremisesId} - {PremisesName}", premises.Id, premises.Name);
        }
        
        return RedirectToAction(nameof(Index));
    }

    private bool PremisesExists(int id)
    {
        return _context.Premises.Any(e => e.Id == id);
    }
}