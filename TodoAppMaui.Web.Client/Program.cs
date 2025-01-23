using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TodoAppMaui.Shared.Services;
using TodoAppMaui.Web.Client.Services;

namespace TodoAppMaui.Web.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddBlazoredLocalStorage();

            // Add device-specific services used by the TodoAppMaui.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddScoped<IToDoService, ToDoServiceWebClient>();

            await builder.Build().RunAsync();
        }
    }
}
