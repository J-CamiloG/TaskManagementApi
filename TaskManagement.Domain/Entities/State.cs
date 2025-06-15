namespace TaskManagement.Domain.Entities;

public class State
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navegacion
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}