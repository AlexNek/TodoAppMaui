
using TodoAppMaui.Shared.Models;

namespace TodoAppMaui.Shared.Services;

public interface IToDoService
{
    Task<List<ToDoItem>> GetToDosAsync();
    Task AddToDoAsync(ToDoItem toDo);
    Task UpdateToDoAsync(ToDoItem toDo);
    Task DeleteToDoAsync(Guid id);
}
