namespace oop_s2_2_mvc_78853.Models;

public class DashboardView
{
    public int TotalPremises { get; set; }
    public int InspectionsThisMonth { get; set; }
    public int FailedInspectionsThisMonth { get; set; }
    public int OverdueFollowUps { get; set; }
    
    public List<string> Towns { get; set; } = new();
    public List<string> RiskRatings { get; set; } = new() { "All", "Low", "Medium", "High" };

    public List<Premises> FilteredPremises { get; set; } = new();
}