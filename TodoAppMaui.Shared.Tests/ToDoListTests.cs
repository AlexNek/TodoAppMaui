using Bunit;

using FluentAssertions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

using Moq;

using Radzen;

using TodoAppMaui.Shared.Components;
using TodoAppMaui.Shared.Models;
using TodoAppMaui.Shared.Services;

using Xunit.Abstractions;

namespace TodoAppMaui.Shared.Tests
{
    public class ToDoListTests : IDisposable
    {
        private readonly TestContext _context;

        private readonly Mock<IJSRuntime> _jsRuntimeMock;

        private readonly ITestOutputHelper _output;

        private readonly Mock<IToDoService> _todoServiceMock;

        public ToDoListTests(ITestOutputHelper output)
        {
            _context = new TestContext();
            _output = output;
            _todoServiceMock = new Mock<IToDoService>();
            _jsRuntimeMock = new Mock<IJSRuntime>();

            // Register services
            _context.Services.AddRadzenComponents();
            _context.Services.AddSingleton(_todoServiceMock.Object);
            _context.Services.AddSingleton(_jsRuntimeMock.Object);

            // Setup JS Interop
            _context.JSInterop.Mode = JSRuntimeMode.Loose;

            // Setup Radzen module
            _context.JSInterop.SetupModule("_content/Radzen.Blazor/Radzen.Blazor.js");

            //// Setup module import - no need
            //var mockJsObjectReference = Mock.Of<IJSObjectReference>();
            //JSInterop.Setup<IJSObjectReference>("import", "./_content/Radzen.Blazor/Radzen.Blazor.js")
            //    .SetResult(mockJsObjectReference);

            // Setup stylesheets
            var cssFile = "_content/Radzen.Blazor/css/default-base.css";
            _context.JSInterop.Setup<string>("Radzen.loadStylesheets", _ => true).SetResult("true");
            _context.JSInterop.Setup<string>("Radzen.loadStylesheet", cssFile).SetResult("true");

            // Setup window width
            _jsRuntimeMock
                .Setup(x => x.InvokeAsync<int>("todoApp.getWindowWidth", It.IsAny<object[]>()))
                .Returns(ValueTask.FromResult(1024));
        }

        [Fact]
        public async Task AddTodo_ShouldCancelAddCorrectly()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);

            var cut = CreateClassUnderTest();

            // Act
            var addButton = cut.Find("[data-testid='add-todo-button']");

            // Assert button is enabled at first
            addButton.ClassName.Should().NotContain("rz-state-disabled");

            await addButton.ClickAsync(new MouseEventArgs());

            cut.WaitForAssertion(
                () =>
                    cut.Find("[data-testid='cancel-edit-button']").Should().NotBeNull());

            var cancelButton = cut.Find("[data-testid='cancel-edit-button']");
            await cancelButton.ClickAsync(new MouseEventArgs());

            // Assert
            addButton.ClassName.Should().NotContain("rz-state-disabled");

            var rows = cut.FindAll(".rz-grid-table .rz-data-row:not(.rz-datatable-edit)");
            rows.Should().HaveCount(testTodos.Count);
        }

        [Fact]
        public async Task AddTodo_ShouldCreateNewTodo_Successfully()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);

            var initialTodosCount = testTodos.Count;

            var cut = CreateClassUnderTest();

            //WriteHtmlToFile(cut, "AddTodo-init.html");
            // Act
            var addButton = cut.Find("[data-testid='add-todo-button']");

            await addButton.ClickAsync(new MouseEventArgs());

            //WriteHtmlToFile(cut, "AddTodo-afteradd.html");
            var saveButton = cut.Find("[data-testid='save-todo-button']");

            await saveButton.ClickAsync(new MouseEventArgs());

            //WriteHtmlToFile(cut, "AddTodo-aftercheck.html");

            // Assert
            var todoItems = cut.FindAll(".rz-grid-table .rz-data-row:not(.rz-datatable-edit)");

            todoItems.Should().HaveCount(initialTodosCount + 1);

            _todoServiceMock.Verify(x => x.AddToDoAsync(It.IsAny<ToDoItem>()), Times.Once);
        }

        [Fact]
        public async Task AddTodo_ShouldDisableAddButtonWhileAdding()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);

            var cut = CreateClassUnderTest();

            // Act
            var addButton = cut.Find("[data-testid='add-todo-button']");

            // Initial check: Ensure the button is enabled (not disabled) at first
            addButton.ClassName.Should().NotContain("rz-state-disabled");

            // Simulate clicking the "Add Todo" button
            await addButton.ClickAsync(new MouseEventArgs());

            // Assert: Verify that the button becomes disabled after clicking
            addButton = cut.Find("[data-testid='add-todo-button']");
            addButton.ClassName.Should().Contain("rz-state-disabled");
        }

        [Fact]
        public async Task AddTodo_ShouldEnableAddButtonAfterCancel()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);

            var cut = CreateClassUnderTest();

            // Act
            var addButton = cut.Find("[data-testid='add-todo-button']");

            // Assert button is enabled at first
            addButton.ClassName.Should().NotContain("rz-state-disabled");

            await addButton.ClickAsync(new MouseEventArgs());
            //WriteHtmlToFile(cut, "AddTodo-afterpressadd.html");
            
            // Wait for the edit row to appear
            cut.WaitForAssertion(() => 
                {
                    cut.Find(".rz-datatable-edit");
                    cut.Find("[data-testid='edit-title-input']");
                    cut.Find("[data-testid='edit-description-input']");
                });

            var cancelButton = cut.Find("[data-testid='cancel-edit-button']");
            await cancelButton.ClickAsync(new MouseEventArgs());

            // Assert
            addButton = cut.Find("[data-testid='add-todo-button']");
            addButton.ClassName.Should().NotContain("rz-state-disabled");
        }

        [Fact]
        public async Task AddTodo_ShouldEnableAddButtonAfterCheck()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);

            var cut = CreateClassUnderTest();

            // Act
            var addButton = cut.Find("[data-testid='add-todo-button']");

            // Assert button is enabled at first
            addButton.ClassName.Should().NotContain("rz-state-disabled");

            await addButton.ClickAsync(new MouseEventArgs());

            // Wait for the edit row to appear
            cut.WaitForAssertion(() => 
                {
                    cut.Find(".rz-datatable-edit");
                    cut.Find("[data-testid='edit-title-input']");
                    cut.Find("[data-testid='edit-description-input']");
                });

            var checkButton = cut.Find("[data-testid='save-todo-button']");
            await checkButton.ClickAsync(new MouseEventArgs());

            // Assert
            addButton = cut.Find("[data-testid='add-todo-button']");
            addButton.ClassName.Should().NotContain("rz-state-disabled");
        }

        [Fact]
        public void Component_ShouldDisplayTodos_WhenLoaded()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);

            // Act
            var cut = CreateClassUnderTest();

            // Assert
            var rows = cut.FindAll(
                "[data-testid='todo-grid'] .rz-data-row:not(.rz-datatable-edit)");
            rows.Should().HaveCount(testTodos.Count);

            var firstRowCells = rows[0].QuerySelectorAll("td");
            firstRowCells.Should().Contain(cell => cell.TextContent.Contains(testTodos[0].Title));
        }

        [Fact]
        public void Component_ShouldRender_Successfully()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);

            // Act
            var cut = CreateClassUnderTest();
            //WriteHtmlToFile(cut,"Component_ShouldRender_Successfully.html");
            
            // Assert
            cut.FindAll(".todo-actions").Should().HaveCount(1);
            cut.FindAll("[data-testid='todo-grid']").Should().HaveCount(1);
        }

        [Fact]
        public async Task DeleteTodo_ShouldRemoveTodo_Successfully()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);
            var todoToDelete = testTodos[0];

            var initialTodos = new List<ToDoItem>(testTodos);
            var cut = CreateClassUnderTest();

            // Act
            var deleteButton = cut.FindAll(
                "[data-testid='delete-todo-button']").FirstOrDefault();

            deleteButton.Should().NotBeNull();
            
            await deleteButton.ClickAsync(new MouseEventArgs());

            // Assert
            _todoServiceMock.Verify(x => x.DeleteToDoAsync(todoToDelete.Id), Times.Once);

            var rows = cut.FindAll(
                "[data-testid='todo-grid'] .rz-data-row:not(.rz-datatable-edit)");
            rows.Should().HaveCount(initialTodos.Count - 1);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task EditTodo_ShouldCancelEditCorrectly()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);
            var todoToEdit = testTodos[0];
            const string updatedTitle = "Updated Title";

            var cut = CreateClassUnderTest();

            // Act
            var editButton = cut.Find("[data-testid='edit-todo-button']");
            await editButton.ClickAsync(new MouseEventArgs());

            //WriteHtmlToFile(cut, "EditTodo-init.html");
            
            cut.WaitForAssertion(() => { cut.Find("[data-testid='edit-title-input']"); });

            // Find and update the title input
            var titleInput = cut.Find("[data-testid='edit-title-input']");
            await titleInput.ChangeAsync(new ChangeEventArgs { Value = updatedTitle });

            var cancelButton = cut.Find("[data-testid='cancel-edit-button']");
            await cancelButton.ClickAsync(new MouseEventArgs());

            // Assert
            var rows = cut.FindAll(
                "[data-testid='todo-grid'] .rz-data-row:not(.rz-datatable-edit)");
            rows.Should().HaveCount(testTodos.Count);

            _todoServiceMock.Verify(
                x => x.UpdateToDoAsync(
                    It.Is<ToDoItem>(
                        t => t.Id == todoToEdit.Id && t.Title == updatedTitle)),
                Times.Never);

            var firstRowCells = rows[0].QuerySelectorAll("td");
            firstRowCells.Should().Contain(cell => cell.TextContent.Contains(testTodos[0].Title));
        }

        [Fact]
        public async Task EditTodo_ShouldUpdateTodo_Successfully()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);
            var todoToEdit = testTodos[0];
            const string updatedTitle = "Updated Title";

            var cut = CreateClassUnderTest();

            // Act
            var editButton = cut.Find("[data-testid='edit-todo-button']");
            await editButton.ClickAsync(new MouseEventArgs());

            cut.WaitForAssertion(() => { cut.Find("[data-testid='edit-title-input']"); });

            // Find and update the title input
            var titleInput = cut.Find("[data-testid='edit-title-input']");
            await titleInput.ChangeAsync(new ChangeEventArgs { Value = updatedTitle });

            var saveButton = cut.Find("[data-testid='save-todo-button']");
            await saveButton.ClickAsync(new MouseEventArgs());

            // Assert
            _todoServiceMock.Verify(
                x => x.UpdateToDoAsync(
                    It.Is<ToDoItem>(
                        t => t.Id == todoToEdit.Id && t.Title == updatedTitle)),
                Times.Once);
        }

        [Fact]
        public void ResponsiveLayout_ShouldHideCreationDate_OnMobileWidth()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);

            _jsRuntimeMock
                .Setup(x => x.InvokeAsync<int>("todoApp.getWindowWidth", It.IsAny<object[]>()))
                .Returns(ValueTask.FromResult(500)); // Mobile width

            // Act
            var cut = CreateClassUnderTest();

            // Assert
            var creationDateColumn = cut.FindAll("[data-testid='column-title']")
                .FirstOrDefault(th => th.TextContent.Contains("Created On"));
            creationDateColumn.Should().BeNull();
        }

        [Theory]
        [InlineData("Test", 2)] // Should find both todos
        [InlineData("Todo 1", 1)] // Should find only one todo
        [InlineData("NonExistent", 0)] // Should find no todos
        public async Task Search_ShouldFilterTodos_Correctly(string searchTerm, int expectedCount)
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);
            var cut = CreateClassUnderTest();

            // Act
            var searchBox = cut.Find("[data-testid='search-todo-input']");
            await searchBox.InputAsync(new ChangeEventArgs { Value = searchTerm });

            // Assert
            var rows = cut.FindAll(
                "[data-testid='todo-grid'] .rz-data-row:not(.rz-datatable-edit)");
            rows.Should().HaveCount(expectedCount);
        }

        [Fact]
        public void ShouldDisplayEmptyList()
        {
            // Arrange
            var testTodos = new List<ToDoItem>();
            SetupTodoService(testTodos);

            var cut = CreateClassUnderTest();

            // Assert
            var todoItems = cut.FindAll(
                "[data-testid='todo-grid'] .rz-data-row:not(.rz-datatable-edit)");
            todoItems.Should().HaveCount(0);
        }

        [Fact(Skip = "Not possible without correct CSS")]
        public async Task ShouldHandleLongTitlesAndDescriptions()
        {
            // Arrange
            var testTodos = new List<ToDoItem>
                                {
                                    new()
                                        {
                                            Id = Guid.NewGuid(),
                                            Title =
                                                "This is a very long title that should not break the layout and it should be displayed correctly without any visual bugs. This is to test the correct usage of styling in components.",
                                            Description =
                                                "This is a very long description that should not break the layout and it should be displayed correctly without any visual bugs. This is to test the correct usage of styling in components. We must also add that the implementation should not generate any errors when loading the page.",
                                            IsCompleted = false,
                                            CreationDate = DateTime.Now.AddDays(-1)
                                        },
                                    new()
                                        {
                                            Id = Guid.NewGuid(),
                                            Title = "This is a test title",
                                            Description = "This is a description",
                                            IsCompleted = true,
                                            CreationDate = DateTime.Now
                                        }
                                };
            SetupTodoService(testTodos);

            // Act
            var cut = CreateClassUnderTest();
            await Task.Delay(100);

            // Assert
            var rows = cut.FindAll(
                "[data-testid='todo-grid'] .rz-data-row:not(.rz-datatable-edit)");
            rows.Should().HaveCount(testTodos.Count);
        }

        [Fact]
        public void ShouldLoadTodos_Successfully()
        {
            // Arrange
            var testTodos = CreateTestTodos();
            SetupTodoService(testTodos);

            // Act
            var cut = CreateClassUnderTest();

            //WriteHtmlToFile(cut, "InitialLoad.html");
            // Assert
            var todoItems = cut.FindAll("[data-testid='todo-grid'] .rz-data-row");

            _output.WriteLine(
                $"Found {todoItems.Count} elements with selector '[data-testid='todo-grid'] .rz-data-row'");

            todoItems.Should().HaveCount(testTodos.Count);
        }

        [Fact]
        public async Task ShouldPaginateCorrectly()
        {
            // Arrange
            var testTodos = new List<ToDoItem>();
            for (int i = 0; i < 25; i++)
            {
                testTodos.Add(
                    new ToDoItem
                        {
                            Id = Guid.NewGuid(),
                            Title = $"Test Todo {i}",
                            Description = $"Description {i}",
                            IsCompleted = false,
                            CreationDate = DateTime.Now.AddDays(-1)
                        });
            }

            SetupTodoService(testTodos);
            var cut = CreateClassUnderTest();

            //WriteHtmlToFile(cut, "Paging.html");
            // Act
            var firstPageRows = cut.FindAll(
                "[data-testid='todo-grid'] .rz-data-row:not(.rz-datatable-edit)");
            firstPageRows.Should().HaveCount(10); // Check that we start with 10 elements

            var nextButton = cut.Find(".rz-pager-next");
            await nextButton.ClickAsync(new MouseEventArgs());

            var secondPageRows = cut.FindAll(
                "[data-testid='todo-grid'] .rz-data-row:not(.rz-datatable-edit)");
            secondPageRows.Should().HaveCount(10);

            await nextButton.ClickAsync(new MouseEventArgs());
            var thirdPageRows = cut.FindAll(
                "[data-testid='todo-grid'] .rz-data-row:not(.rz-datatable-edit)");
            thirdPageRows.Should().HaveCount(5);
        }

        public static void WriteHtmlToFile(IRenderedComponent<ToDoList> component, string filePath)
        {
            var html = component.Markup;
            File.WriteAllText(filePath, html);
        }

        private IRenderedComponent<ToDoList> CreateClassUnderTest()
        {
            return _context.RenderComponent<ToDoList>();
        }

        private List<ToDoItem> CreateTestTodos()
        {
            return new List<ToDoItem>
                       {
                           new ToDoItem
                               {
                                   Id = Guid.NewGuid(),
                                   Title = "Test Todo 1",
                                   Description = "Description 1",
                                   IsCompleted = false
                               },
                           new ToDoItem
                               {
                                   Id = Guid.NewGuid(),
                                   Title = "Test Todo 2",
                                   Description = "Description 2",
                                   IsCompleted = true
                               }
                       };
        }

        private void SetupTodoService(List<ToDoItem> todos)
        {
            _todoServiceMock
                .Setup(x => x.GetToDosAsync())
                .ReturnsAsync(() => new List<ToDoItem>(todos));

            _todoServiceMock
                .Setup(x => x.AddToDoAsync(It.IsAny<ToDoItem>()))
                .Returns(
                    async (ToDoItem newTodo) =>
                        {
                            todos.Add(newTodo);
                            return true;
                        });

            _todoServiceMock
                .Setup(x => x.DeleteToDoAsync(It.IsAny<Guid>()))
                .Returns(
                    async (Guid idToDelete) =>
                        {
                            var removedCount = todos.RemoveAll(x => x.Id == idToDelete);
                            return removedCount > 0; // Return true if an item was removed
                        });

            _todoServiceMock
                .Setup(x => x.UpdateToDoAsync(It.IsAny<ToDoItem>()))
                .Returns(
                    async (ToDoItem updatedTodo) =>
                        {
                            var index = todos.FindIndex(x => x.Id == updatedTodo.Id);
                            if (index != -1)
                            {
                                todos[index] = updatedTodo;
                                return true;
                            }

                            return false; // Return false if the todo was not found
                        });
        }
    }
}
