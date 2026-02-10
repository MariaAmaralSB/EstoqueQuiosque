using EstoqueQuiosque.App.Pages;
using EstoqueQuiosque.App.Services;
using EstoqueQuiosque.App.ViewModels;
using Microsoft.Extensions.Logging;

namespace EstoqueQuiosque.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<EstoqueService>();
        builder.Services.AddSingleton<EstoqueViewModel>();
        builder.Services.AddSingleton<EstoquePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
