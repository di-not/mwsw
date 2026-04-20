namespace MWSW.Backend.Models;

public class CartItem
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int PlanetId { get; set; }
    public int Quantity { get; set; }
    public Planet? Planet { get; set; }
}