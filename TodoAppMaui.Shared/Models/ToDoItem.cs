namespace TodoAppMaui.Shared.Models;

public class ToDoItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
}