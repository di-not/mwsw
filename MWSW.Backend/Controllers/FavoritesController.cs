using Microsoft.AspNetCore.Mvc;
using MWSW.Backend.Services;
using MWSW.Backend.Models;
using System.Text.Json;

namespace MWSW.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FavoritesController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public FavoritesController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserFavorites(int userId)
    {
        try
        {
            var favorites = await _databaseService.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserFavorites: {ex.Message}");
            return StatusCode(500, new { message = $"Ошибка: {ex.Message}" });
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToFavorites([FromBody] FavoriteRequest request)
    {
        try
        {
            Console.WriteLine($"Received add to favorites request: userId={request?.userId}, planetId={request?.planetId}");
            
            if (request == null)
            {
                return BadRequest(new { message = "Неверный формат запроса" });
            }
            
            if (request.userId <= 0)
            {
                return BadRequest(new { message = "Неверный ID пользователя" });
            }
            
            if (request.planetId <= 0)
            {
                return BadRequest(new { message = "Неверный ID планеты" });
            }
            
            var result = await _databaseService.AddToFavoritesAsync(request.userId, request.planetId);
            if (result)
                return Ok(new { message = "Планета добавлена в избранное" });
            
            return BadRequest(new { message = "Ошибка при добавлении в избранное (возможно, уже есть)" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddToFavorites: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = $"Ошибка сервера: {ex.Message}" });
        }
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveFromFavorites([FromQuery] int userId, [FromQuery] int planetId)
    {
        try
        {
            Console.WriteLine($"Received remove from favorites request: userId={userId}, planetId={planetId}");
            
            if (userId <= 0)
            {
                return BadRequest(new { message = "Неверный ID пользователя" });
            }
            
            if (planetId <= 0)
            {
                return BadRequest(new { message = "Неверный ID планеты" });
            }
            
            var result = await _databaseService.RemoveFromFavoritesAsync(userId, planetId);
            if (result)
                return Ok(new { message = "Планета удалена из избранного" });
            
            return NotFound(new { message = "Планета не найдена в избранном" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RemoveFromFavorites: {ex.Message}");
            return StatusCode(500, new { message = $"Ошибка сервера: {ex.Message}" });
        }
    }

    [HttpGet("check/{userId}/{planetId}")]
    public async Task<IActionResult> IsFavorite(int userId, int planetId)
    {
        try
        {
            var isFavorite = await _databaseService.IsFavoriteAsync(userId, planetId);
            return Ok(new { isFavorite });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in IsFavorite: {ex.Message}");
            return StatusCode(500, new { message = $"Ошибка сервера: {ex.Message}" });
        }
    }
}

public class FavoriteRequest
{
    public int userId { get; set; }
    public int planetId { get; set; }
}