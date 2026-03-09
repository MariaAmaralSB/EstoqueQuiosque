using EstoqueQuiosque.App.Pages;
using EstoqueQuiosque.App.Services;
using EstoqueQuiosque.App.ViewModels;

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
                fonts.AddFont("Syne-Bold.ttf", "SyneBold");
                fonts.AddFont("DMSans-Regular.ttf", "DMSansRegular");
            });

        var supabaseOptions = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };
        var supabase = new Supabase.Client(SupabaseConfig.Url, SupabaseConfig.Key, supabaseOptions);

        builder.Services.AddSingleton(supabase);
        builder.Services.AddSingleton<EstoqueService>();
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<EstoqueViewModel>();
        builder.Services.AddSingleton<CadastroProdutoViewModel>();
        builder.Services.AddSingleton<MovimentosViewModel>();
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<EstoquePage>();
        builder.Services.AddSingleton<CadastroProdutoPage>();
        builder.Services.AddSingleton<MovimentosPage>();

        return builder.Build();
    }
}
