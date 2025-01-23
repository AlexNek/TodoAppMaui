using Microsoft.JSInterop;

using Radzen;
using Radzen.Blazor;

using TodoAppMaui.Shared.Models;

namespace TodoAppMaui.Shared.Components;

public partial class ToDoList
{
    private const int MOBILE_WIDTH_THRESHOLD = 640;

    #region Properties

    private RadzenDataGrid<ToDoItem> TodoGrid { get; set; }

    private List<ToDoItem> Todos { get; set; } = new();

    private List<ToDoItem> FilteredTodos { get; set; } = new();

    private bool ShowCreationDate { get; set; } = true;

    private string SearchTerm { get; set; } = "";

    private bool IsAdding { get; set; }

    private DotNetObjectReference<ToDoList> _dotNetRef;

    private string GridClass => IsHeaderVisible ? string.Empty : "hide-headers";

    private bool IsHeaderVisible = false; // Dynamically toggle this

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await RefreshTodoList();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeWindowResizeHandler();
        }
    }

    private void OnCellRender(DataGridCellRenderEventArgs<ToDoItem> args)
    {
        if (args.Data.IsCompleted)
        {
            args.Attributes.Add("class", "completed-row");
        }
    }

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }

    #endregion

    #region Window Resize Handling

    private async Task InitializeWindowResizeHandler()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync(
            "todoApp.addWindowResizeListener",
            _dotNetRef);
        await UpdateColumnVisibility();
    }

    private async Task UpdateColumnVisibility()
    {
        var width = await JSRuntime.InvokeAsync<int>("todoApp.getWindowWidth", new object[] { });
        SetColumnVisibility(width);
    }

    private void SetColumnVisibility(int width)
    {
        var newValue = width > MOBILE_WIDTH_THRESHOLD;
        if (ShowCreationDate != newValue)
        {
            ShowCreationDate = newValue;

            InvokeAsync(StateHasChanged);
        }
    }

    [JSInvokable]
    public void OnWindowResize(int width) => SetColumnVisibility(width);

    #endregion

    #region Todo Operations

    private async Task RefreshTodoList()
    {
        Todos = await ToDoService.GetToDosAsync();
        ApplyFilter();
    }

    private async Task AddTodo()
    {
        if (IsAdding)
        {
            return;
        }

        IsAdding = true;

        var newTodo = new ToDoItem
                          {
                              Id = Guid.Empty,
                              Title = "New Todo",
                              Description = "",
                              IsCompleted = false,
                              CreationDate = DateTime.Now
                          };

        // Add the new item to the beginning of the list
        Todos.Insert(0, newTodo);

        ApplyFilter();
        // Update the grid to include the new item
        await TodoGrid.Reload();
        // Start editing the newly added row
        await TodoGrid.EditRow(newTodo);
    }

    private async Task EditRow(ToDoItem todo) => await TodoGrid.EditRow(todo);

    private async Task SaveRow(ToDoItem todo)
    {
        if (todo.Id == Guid.Empty)
        {
            await SaveNewTodo(todo);
        }
        else
        {
            await UpdateExistingTodo(todo);
        }

        await FinalizeSave(todo);
    }

    private async Task SaveNewTodo(ToDoItem todo)
    {
        // Ensure a new unique ID
        todo.Id = Guid.NewGuid();
        await ToDoService.AddToDoAsync(todo);

        // Remove the temporary item from its current position
        Todos.RemoveAll(t => t.Id == todo.Id);

        // Add the new item to the end of the list
        Todos.Add(todo);

        // Re-apply the filter to update FilteredTodos
        ApplyFilter();
    }

    private async Task UpdateExistingTodo(ToDoItem todo)
    {
        await ToDoService.UpdateToDoAsync(todo);
    }

    private async Task FinalizeSave(ToDoItem todo)
    {
        await RefreshTodoList();
        // Exit edit mode for the row
        TodoGrid.CancelEditRow(todo);
        IsAdding = false;
        // force a re-render
        await TodoGrid.Reload();
    }

    private async Task CancelEdit(ToDoItem todo)
    {
        if (todo.Id == Guid.Empty)
        {
            Todos.RemoveAll(t => t.Id == todo.Id);
            ApplyFilter();
            await TodoGrid.Reload();
        }

        TodoGrid.CancelEditRow(todo);
        IsAdding = false;
    }

    private async Task DeleteTodo(Guid id)
    {
        await ToDoService.DeleteToDoAsync(id);
        await RefreshTodoList();
    }

    private async Task OnIsCompletedChanged(ToDoItem todo, object value)
    {
        if (value is bool isCompleted)
        {
            todo.IsCompleted = isCompleted;
            await ToDoService.UpdateToDoAsync(todo);
            await RefreshTodoList();
        }
    }

    #endregion

    #region Filtering

    private void OnSearch(string value)
    {
        SearchTerm = value;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredTodos = Todos.Where(
            todo =>
                string.IsNullOrWhiteSpace(SearchTerm) ||
                todo.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                todo.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                todo.Id == Guid.Empty
        ).ToList();
    }

    #endregion
}
