namespace TaskScheduler.MinimalAPI.Models.DTOs.V2;

public class TaskResponseV2
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public string Category { get; set; } = string.Empty;      // Поле 1
    public Guid? AssignedToUserId { get; set; }               // Поле 2
}