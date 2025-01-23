using Blazored.LocalStorage;

using TodoAppMaui.Shared.Services;
using TodoAppMaui.Web.Client.Services;
using TodoAppMaui.Web.Components;
using TodoAppMaui.Web.Services;

using FormFactor = TodoAppMaui.Web.Services.FormFactor;

namespace TodoAppMaui
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddBlazoredLocalStorage();

            // Add device-specific services used by the TodoAppMaui.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddScoped<IToDoService, ToDoServiceServer>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(
                    typeof(TodoAppMaui.Shared._Imports).Assembly,
                    typeof(TodoAppMaui.Web.Client._Imports).Assembly);

            app.Run();
        }
    }
}
