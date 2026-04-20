using Microsoft.AspNetCore.Mvc;
using MWSW.Backend.Services;

namespace MWSW.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public NewsController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNews()
    {
        var news = await _databaseService.GetAllNewsAsync();
        return Ok(news);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNewsById(int id)
    {
        var news = await _databaseService.GetNewsByIdAsync(id);
        if (news == null)
            return NotFound(new { message = "Новость не найдена" });
        
        return Ok(news);
    }
}