using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_78853.Data;
using Serilog;

namespace oop_s2_2_mvc_78853.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string town, string riskRating)
    {
        var inspectionsQuery = _context.Inspections
            .Include(i => i.Premises)
            .AsQueryable();

        var followUpsQuery = _context.FollowUps
            .Include(f => f.Inspection)
            .ThenInclude(i => i.Premises)
            .AsQueryable();

        if (!string.IsNullOrEmpty(town) && town != "All")
        {
            inspectionsQuery = inspectionsQuery.Where(i => i.Premises.Town == town);
            followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.Town == town);
        }

        if (!string.IsNullOrEmpty(riskRating) && riskRating != "All")
        {
            inspectionsQuery = inspectionsQuery.Where(i => i.Premises.RiskRating == riskRating);
            followUpsQuery = followUpsQuery.Where(f => f.Inspection.Premises.RiskRating == riskRating);
        }

        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var dashboardData = new
        {
            TotalPremises = await _context.Premises.CountAsync(),
            
            InspectionsThisMonth = await inspectionsQuery
                .CountAsync(i => i.InspectionDate >= startOfMonth),
            
            FailedInspectionsThisMonth = await inspectionsQuery
                .CountAsync(i => i.InspectionDate >= startOfMonth && i.Outcome == "Fail"),
            
            OverdueFollowUps = await followUpsQuery
                .CountAsync(f => f.Status == "Open" && f.DueDate < DateTime.Today),
            
            Towns = await _context.Premises
                .Select(p => p.Town)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync(),
            
            RiskRatings = new[] { "All", "Low", "Medium", "High" }
        };

        ViewBag.SelectedTown = town ?? "All";
        ViewBag.SelectedRiskRating = riskRating ?? "All";

        Log.Information("Dashboard viewed with filters - Town: {Town}, RiskRating: {RiskRating}", 
            town ?? "All", riskRating ?? "All");

        return View(dashboardData);
    }
}