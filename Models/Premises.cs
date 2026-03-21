namespace oop_s2_2_mvc_78853.Models;

public class Premises
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string RiskRating { get; set; } = string.Empty;

    public List<Inspection> Inspections { get; set; } = new();
}