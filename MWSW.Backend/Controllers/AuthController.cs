// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using MWSW.Backend.Services;
using MWSW.Backend.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MWSW.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private static readonly HashSet<string> AdminEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin@example.com",
        "ivan@example.com"
    };

    public AuthController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email и пароль обязательны" });
            }

            var user = await _databaseService.GetUserByEmailAsync(request.Email);
            
            if (user == null)
            {
                return Unauthorized(new { message = "Неверный email или пароль" });
            }
            
            var passwordHash = HashPassword(request.Password);
            
            if (user.PasswordHash != passwordHash)
            {
                return Unauthorized(new { message = "Неверный email или пароль" });
            }
            
            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phone = user.Phone,
                birthDate = user.BirthDate,
                registrationDate = user.RegistrationDate,
                avatarColor = user.AvatarColor,
                isAdmin = IsAdminEmail(user.Email),
                message = "Вход выполнен успешно"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return StatusCode(500, new { message = $"Ошибка при входе: {ex.Message}" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            Console.WriteLine($"Received registration request: {JsonSerializer.Serialize(request)}");
            
            if (request == null)
            {
                Console.WriteLine("Request is null");
                return BadRequest(new { message = "Неверный формат запроса" });
            }

            if (string.IsNullOrEmpty(request.FullName))
            {
                return BadRequest(new { message = "Имя обязательно" });
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email обязателен" });
            }

            if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6)
            {
                return BadRequest(new { message = "Пароль должен быть не менее 6 символов" });
            }
            
            // Проверяем, не существует ли пользователь
            var existingUser = await _databaseService.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }
            
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                BirthDate = request.BirthDate,
                AvatarColor = GetRandomColor()
            };
            
            var passwordHash = HashPassword(request.Password);
            var result = await _databaseService.CreateUserAsync(user, passwordHash);
            
            if (result)
            {
                Console.WriteLine($"User registered successfully: {request.Email}");
                return Ok(new { message = "Регистрация прошла успешно" });
            }
            
            return BadRequest(new { message = "Ошибка при регистрации" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = $"Ошибка при регистрации: {ex.Message}" });
        }
    }
    
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    private string GetRandomColor()
    {
        var colors = new[] { "#3498db", "#e74c3c", "#2ecc71", "#f39c12", "#9b59b6", "#1abc9c" };
        var random = new Random();
        return colors[random.Next(colors.Length)];
    }

    private static bool IsAdminEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return AdminEmails.Contains(email.Trim());
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
}
