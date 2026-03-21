namespace oop_s2_2_mvc_78853.Models;

public class FollowUp
{
    public int Id { get; set; }
    
    public int InspectionId { get; set; }
    public Inspection? Inspection { get; set; }
    
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime? ClosedDate { get; set; }
}