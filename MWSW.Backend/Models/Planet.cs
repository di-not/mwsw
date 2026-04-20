namespace MWSW.Backend.Models;

public class Planet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public string? Mass { get; set; }
    public string? Diameter { get; set; }
    public string? DistanceFromSun { get; set; }
    public string? Type { get; set; }
    public int? Moons { get; set; }
    public string? OrbitalPeriod { get; set; }
    public bool SoldOut { get; set; }
}