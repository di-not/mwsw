// Models/User.cs
namespace MWSW.Backend.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? AvatarColor { get; set; }
}