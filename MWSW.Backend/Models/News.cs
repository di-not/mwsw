namespace MWSW.Backend.Models;

public class News
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Excerpt { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishDate { get; set; }
    public DateTime CreatedAt { get; set; }
}