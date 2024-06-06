using System.ComponentModel.DataAnnotations;

namespace TODOList.Models;

public class TodoModel
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsDone { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
