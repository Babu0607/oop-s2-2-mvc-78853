namespace oop_s2_2_mvc_78853.Models;

public class Inspection
{
    public int Id { get; set; }
    
    public int PremisesId { get; set; }
    public Premises? Premises { get; set; }
    
    public DateTime InspectionDate { get; set; }
    public int Score { get; set; }
    public string Outcome { get; set; } = "Pass";
    public string? Notes { get; set; }
    
    public List<FollowUp> FollowUps { get; set; } = new();
}