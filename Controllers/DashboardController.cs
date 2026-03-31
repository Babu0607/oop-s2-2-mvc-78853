using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_78853.Data;
using oop_s2_2_mvc_78853.Models;
using Serilog;

namespace oop_s2_2_mvc_78853.Controllers;

[Authorize(Roles = "Admin,Inspector,Viewer")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string town, string riskRating)
    {
        var premisesQuery = _context.Premises.AsQueryable(); 
        var inspectionsQuery = _context.Inspections.Include(i => i.Premises).AsQueryable(); 
        var followUpsQuery = _context.FollowUps.Include(f => f.Inspection).ThenInclude(i => i.Premises).AsQueryable();

        if (!string.IsNullOrEmpty(town) && town != "All") 
        {
            premisesQuery = premisesQuery.Where(p => p.Town == town);
            inspectionsQuery = inspectionsQuery.Where(i => i.Premises.Town == town);
            followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.Town == town);
        }

        if (!string.IsNullOrEmpty(riskRating) && riskRating != "All")
        {
            premisesQuery = premisesQuery.Where(p => p.RiskRating == riskRating);
            inspectionsQuery = inspectionsQuery.Where(i => i.Premises.RiskRating == riskRating);
            followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.RiskRating == riskRating);
        }

        var now = DateTime.Now; 
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var viewModel = new DashboardView  
        {
            TotalPremises = await premisesQuery.CountAsync(),
            InspectionsThisMonth = await inspectionsQuery.CountAsync(i => i.InspectionDate >= startOfMonth),
            FailedInspectionsThisMonth = await inspectionsQuery.CountAsync(i => i.InspectionDate >= startOfMonth && i.Outcome == "Fail"),
            OverdueFollowUps = await followUpsQuery.CountAsync(f => f.Status == "Open" && f.DueDate < DateTime.Today),
            
            Towns = await _context.Premises.Select(p => p.Town).Distinct().OrderBy(t => t).ToListAsync(),

            FilteredPremises = await premisesQuery.ToListAsync()
        };

        ViewBag.SelectedTown = town ?? "All";
        ViewBag.SelectedRiskRating = riskRating ?? "All";

        return View(viewModel);
    }
}