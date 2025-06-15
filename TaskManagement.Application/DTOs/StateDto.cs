namespace TaskManagement.Application.DTOs;

public class StateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateStateDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateStateDto
{
    public string Name { get; set; } = string.Empty;
}