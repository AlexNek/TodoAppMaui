using System.Text.Json;

using Blazored.LocalStorage;

using TodoAppMaui.Shared.Models;
using TodoAppMaui.Shared.Services;

namespace TodoAppMaui.Web.Services;

public class ToDoServiceServer : IToDoService
{
    private const string LocalStorageKey = "todos";

    private readonly ILocalStorageService _localStorageService;

    public ToDoServiceServer(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
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
        await Task.CompletedTask;

        // Return an empty list or placeholder data during static rendering
        return Enumerable.Empty<ToDoItem>().ToList();

        //var json = await _localStorageService.GetItemAsync<string>(LocalStorageKey);
        //return json == null
        //           ? new List<ToDoItem>()
        //           : JsonSerializer.Deserialize<List<ToDoItem>>(json) ?? new List<ToDoItem>();
    }

    public async Task UpdateToDoAsync(ToDoItem toDo)
    {
        var todos = await GetToDosAsync();
        var index = todos.FindIndex(t => t.Id == toDo.Id);
        if (index != -1)
        {
            todos[index] = toDo;
            await SaveToDosAsync(todos);
        }
    }

    private async Task SaveToDosAsync(List<ToDoItem> todos)
    {
        var json = JsonSerializer.Serialize(todos);
        await _localStorageService.SetItemAsync(LocalStorageKey, json);
    }
}
