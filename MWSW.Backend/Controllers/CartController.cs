using Microsoft.AspNetCore.Mvc;
using MWSW.Backend.Services;
using MWSW.Backend.Models;
using System.Text.Json;

namespace MWSW.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public CartController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart([FromQuery] int? userId)
    {
        try
        {
            var items = await _databaseService.GetCartItemsAsync(userId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCart: {ex.Message}");
            return StatusCode(500, new { message = $"Ошибка: {ex.Message}" });
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            Console.WriteLine($"Received add to cart request: userId={request?.userId}, planetId={request?.planetId}, quantity={request?.quantity}");
            
            if (request == null)
            {
                return BadRequest(new { message = "Неверный формат запроса" });
            }
            
            if (request.planetId <= 0)
            {
                return BadRequest(new { message = "Неверный ID планеты" });
            }
            
            if (request.quantity <= 0)
            {
                request.quantity = 1;
            }
            
            var result = await _databaseService.AddToCartAsync(request.userId, request.planetId, request.quantity);
            if (result)
                return Ok(new { message = "Товар добавлен в корзину" });
            
            return BadRequest(new { message = "Ошибка при добавлении в корзину" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddToCart: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = $"Ошибка сервера: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        try
        {
            var result = await _databaseService.RemoveFromCartAsync(id);
            if (result)
                return Ok(new { message = "Товар удален из корзины" });
            
            return NotFound(new { message = "Товар не найден" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RemoveFromCart: {ex.Message}");
            return StatusCode(500, new { message = $"Ошибка сервера: {ex.Message}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuantity(int id, [FromBody] UpdateQuantityRequest request)
    {
        try
        {
            Console.WriteLine($"Received update quantity request: id={id}, quantity={request?.quantity}");
            
            if (request == null)
            {
                return BadRequest(new { message = "Неверный формат запроса" });
            }
            
            if (request.quantity <= 0)
            {
                return BadRequest(new { message = "Количество должно быть больше 0" });
            }
            
            var result = await _databaseService.UpdateCartItemQuantityAsync(id, request.quantity);
            if (result)
                return Ok(new { message = "Количество обновлено" });
            
            return NotFound(new { message = "Товар не найден" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateQuantity: {ex.Message}");
            return StatusCode(500, new { message = $"Ошибка сервера: {ex.Message}" });
        }
    }
}

public class AddToCartRequest
{
    public int? userId { get; set; }
    public int planetId { get; set; }
    public int quantity { get; set; } = 1;
}

public class UpdateQuantityRequest
{
    public int quantity { get; set; }
}