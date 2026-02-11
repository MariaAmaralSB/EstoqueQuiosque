namespace EstoqueQuiosque.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Força tema claro para manter contraste e legibilidade.
        UserAppTheme = AppTheme.Light;

        MainPage = new AppShell();
    }
}
