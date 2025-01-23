using Microsoft.Extensions.Logging;

#if ANDROID
using TodoAppMaui.Platforms.Android;
#endif

#if WINDOWS
using TodoAppMaui.Platforms.Windows;
#endif

using TodoAppMaui.Services;
using TodoAppMaui.Shared.Services;

namespace TodoAppMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(
                    fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

            // Add device-specific services used by the TodoAppMaui.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddSingleton<IToDoService, ToDoService>();

//#if WINDOWS
//            builder.Services.AddSingleton<IToDoService, ToDoServiceWIndows>();
//#elif ANDROID
//            builder.Services.AddSingleton<IToDoService, ToDoServiceAndroid>();
//#endif

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
