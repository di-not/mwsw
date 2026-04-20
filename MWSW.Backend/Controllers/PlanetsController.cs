using Microsoft.AspNetCore.Mvc;
using MWSW.Backend.Models;
using MWSW.Backend.Services;
using System.Net.Mail;

namespace MWSW.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlanetsController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    private static readonly HashSet<string> AdminEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin@example.com",
        "ivan@example.com"
    };

    public PlanetsController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Planet>>> GetAllPlanets()
    {
        try
        {
            var planets = await _databaseService.GetAllPlanetsAsync();
            return Ok(planets);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при получении списка планет", error = ex.Message });
        }
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<Planet>> GetPlanetByName(string name)
    {
        try
        {
            var planet = await _databaseService.GetPlanetByNameAsync(name);
            if (planet == null)
                return NotFound(new { message = "Планета не найдена" });

            return Ok(planet);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при получении информации о планете", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlanet([FromBody] CreatePlanetRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    message = "Некорректный JSON запроса",
                    errors = new[] { "Тело запроса пустое или повреждено." }
                });
            }

            var validationErrors = ValidateCreatePlanetRequest(request);
            if (validationErrors.Count > 0)
            {
                return BadRequest(new
                {
                    message = "Проверьте поля формы",
                    errors = validationErrors
                });
            }

            if (!IsAdminEmail(request.AdminEmail))
                return Unauthorized(new { message = "Требуются права администратора" });

            var trimmedName = request.Name.Trim();
            var existingPlanet = await _databaseService.GetPlanetByNameAsync(trimmedName);
            if (existingPlanet != null)
            {
                return Conflict(new
                {
                    message = "Планета с таким названием уже существует",
                    errors = new[] { "Название должно быть уникальным." }
                });
            }

            var planet = new Planet
            {
                Name = trimmedName,
                ShortDescription = NormalizeNullable(request.ShortDescription),
                Description = NormalizeNullable(request.Description),
                ImageUrl = NormalizeNullable(request.ImageUrl),
                Price = request.Price,
                Mass = NormalizeNullable(request.Mass),
                Diameter = NormalizeNullable(request.Diameter),
                DistanceFromSun = NormalizeNullable(request.DistanceFromSun),
                Type = NormalizeNullable(request.Type),
                Moons = request.Moons,
                OrbitalPeriod = NormalizeNullable(request.OrbitalPeriod),
                SoldOut = request.SoldOut
            };

            var newId = await _databaseService.CreatePlanetAsync(planet);
            planet.Id = newId;

            return Ok(new { message = "Planet created successfully", planet });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreatePlanet: {ex.Message}");
            return StatusCode(500, new { message = $"Error creating planet: {ex.Message}" });
        }
    }

    private static List<string> ValidateCreatePlanetRequest(CreatePlanetRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Название планеты обязательно.");
        else if (request.Name.Trim().Length < 2)
            errors.Add("Название должно содержать минимум 2 символа.");
        else if (request.Name.Trim().Length > 50)
            errors.Add("Название не должно превышать 50 символов.");
        else if (!ContainsLetter(request.Name))
            errors.Add("Название должно содержать хотя бы одну букву.");

        if (string.IsNullOrWhiteSpace(request.AdminEmail))
            errors.Add("Email администратора обязателен.");
        else if (!IsValidEmail(request.AdminEmail))
            errors.Add("Email администратора указан в неверном формате.");

        if (string.IsNullOrWhiteSpace(request.ShortDescription))
            errors.Add("Короткое описание обязательно.");
        else if (request.ShortDescription.Trim().Length < 10)
            errors.Add("Короткое описание должно быть не менее 10 символов.");
        else if (!ValidateMaxLength(request.ShortDescription, 65535))
            errors.Add("Короткое описание слишком длинное.");

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add("Полное описание обязательно.");
        else if (request.Description.Trim().Length < 30)
            errors.Add("Полное описание должно быть не менее 30 символов.");
        else if (!ValidateMaxLength(request.Description, 65535))
            errors.Add("Полное описание слишком длинное.");

        if (string.IsNullOrWhiteSpace(request.ImageUrl))
            errors.Add("Ссылка на изображение обязательна.");
        else if (!ValidateMaxLength(request.ImageUrl, 255))
            errors.Add("Ссылка на изображение не должна превышать 255 символов.");

        if (string.IsNullOrWhiteSpace(request.Mass))
            errors.Add("Масса обязательна.");
        else if (!IsPositiveDecimal(request.Mass))
            errors.Add("Масса должна быть положительным числом.");
        else if (!ValidateMaxLength(request.Mass, 50))
            errors.Add("Поле 'Масса' не должно превышать 50 символов.");

        if (string.IsNullOrWhiteSpace(request.Diameter))
            errors.Add("Диаметр обязателен.");
        else if (!IsPositiveDecimal(request.Diameter))
            errors.Add("Диаметр должен быть положительным числом.");
        else if (!ValidateMaxLength(request.Diameter, 50))
            errors.Add("Поле 'Диаметр' не должно превышать 50 символов.");

        if (string.IsNullOrWhiteSpace(request.DistanceFromSun))
            errors.Add("Расстояние от Солнца обязательно.");
        else if (!IsPositiveDecimal(request.DistanceFromSun))
            errors.Add("Расстояние от Солнца должно быть положительным числом.");
        else if (!ValidateMaxLength(request.DistanceFromSun, 50))
            errors.Add("Поле 'Расстояние от Солнца' не должно превышать 50 символов.");

        if (string.IsNullOrWhiteSpace(request.Type))
            errors.Add("Тип планеты обязателен.");
        else if (request.Type.Trim().Length < 3)
            errors.Add("Тип планеты должен содержать минимум 3 символа.");
        else if (!ValidateMaxLength(request.Type, 50))
            errors.Add("Поле 'Тип' не должно превышать 50 символов.");

        if (string.IsNullOrWhiteSpace(request.OrbitalPeriod))
            errors.Add("Период обращения обязателен.");
        else if (!IsPositiveDecimal(request.OrbitalPeriod))
            errors.Add("Период обращения должен быть положительным числом.");
        else if (!IsWholeNumber(request.OrbitalPeriod))
            errors.Add("Период обращения должен быть целым числом.");
        else if (!IsMultipleOf(request.OrbitalPeriod, 10m))
            errors.Add("Период обращения должен быть кратен 10.");
        else if (!ValidateMaxLength(request.OrbitalPeriod, 50))
            errors.Add("Поле 'Период обращения' не должно превышать 50 символов.");

        if (!string.IsNullOrWhiteSpace(request.ImageUrl) &&
            !Uri.TryCreate(request.ImageUrl.Trim(), UriKind.Absolute, out _))
            errors.Add("Ссылка на изображение должна быть корректным абсолютным URL.");
        else if (!string.IsNullOrWhiteSpace(request.ImageUrl) &&
            !request.ImageUrl.Trim().StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !request.ImageUrl.Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            errors.Add("Ссылка на изображение должна начинаться с http:// или https://.");

        if (!request.Price.HasValue)
            errors.Add("Цена обязательна.");
        else if (request.Price.Value <= 0)
            errors.Add("Цена должна быть больше нуля.");
        else if (request.Price.Value < 1000)
            errors.Add("Цена должна быть не менее 1000.");

        if (!request.Moons.HasValue)
            errors.Add("Количество лун обязательно.");
        else if (request.Moons.Value < 0)
            errors.Add("Количество лун не может быть отрицательным.");
        else if (request.Moons.Value > 500)
            errors.Add("Количество лун выглядит некорректным (максимум 500).");

        return errors;
    }

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    private static bool ValidateMaxLength(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        return value.Length <= maxLength;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email.Trim());
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool ContainsLetter(string value)
    {
        foreach (var ch in value)
        {
            if (char.IsLetter(ch))
                return true;
        }

        return false;
    }

    private static bool IsPositiveDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            return false;

        return parsed > 0;
    }

    private static bool IsWholeNumber(string value)
    {
        if (!TryParseInvariantDecimal(value, out var parsed))
            return false;

        return parsed % 1m == 0m;
    }

    private static bool IsMultipleOf(string value, decimal divisor)
    {
        if (divisor == 0m)
            return false;

        if (!TryParseInvariantDecimal(value, out var parsed))
            return false;

        return parsed % divisor == 0m;
    }

    private static bool TryParseInvariantDecimal(string value, out decimal parsed)
    {
        parsed = 0m;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().Replace(',', '.');
        return decimal.TryParse(
            normalized,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out parsed
        );
    }

    private static bool IsAdminEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return AdminEmails.Contains(email.Trim());
    }
}

public class CreatePlanetRequest
{
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
    public string? AdminEmail { get; set; }
}
