using System.Text.Json;
using TodoAppMaui.Shared.Models;
using TodoAppMaui.Shared.Services;

namespace TodoAppMaui.Platforms.Android
{
    /// <summary>
    /// Class ToDoService. ANDROID
    /// Implements the <see cref="IToDoService" />
    /// </summary>
    /// <seealso cref="IToDoService" />
    public class ToDoService : IToDoService
    {
        private readonly string _filePath;

        public ToDoService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "todos.json");
        }

        public async Task AddToDoAsync(ToDoItem toDo)
        {
            var todos = await GetToDosAsync();
            todos.Add(toDo);
            await SaveToDosAsync(todos);
        }

        public async Task DeleteToDoAsync(Guid id)
        {
            var todos = await GetToDosAsync();
            todos.RemoveAll(t => t.Id == id);
            await SaveToDosAsync(todos);
        }

        public async Task<List<ToDoItem>> GetToDosAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<ToDoItem>();
                }

                var json = await File.ReadAllTextAsync(_filePath);
                return string.IsNullOrEmpty(json)
                           ? new List<ToDoItem>()
                           : JsonSerializer.Deserialize<List<ToDoItem>>(json) ?? new List<ToDoItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading todos: {ex.Message}");
                return new List<ToDoItem>();
            }
        }

        public async Task UpdateToDoAsync(ToDoItem toDo)
        {
            try
            {
                var todos = await GetToDosAsync();
                var index = todos.FindIndex(t => t.Id == toDo.Id);
                if (index != -1)
                {
                    todos[index] = toDo;
                    await SaveToDosAsync(todos);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating todo: {ex.Message}");
            }
        }

        private async Task SaveToDosAsync(List<ToDoItem> todos)
        {
            try
            {
                var json = JsonSerializer.Serialize(todos);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving todos: {ex.Message}");
            }
        }
    }
}
