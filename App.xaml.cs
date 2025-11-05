using SRQCC.Services;
using SRQCC.Models;

namespace SRQCC
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // Forzar tema CLARO siempre (Arregla el bug de que con tema oscura desaparece algunos textos)
            Application.Current.UserAppTheme = AppTheme.Light;

            //  CARGAR SESIÓN AL INICIAR LA APP
            _ = LoadSessionAsync();

            //  VERIFICAR ACTUALIZACIONES EN SEGUNDO PLANO
            _ = CheckForUpdatesOnStartup();
        }

        private async Task LoadSessionAsync()
        {
            try
            {
                await SessionService.LoadSession();
                Console.WriteLine($"Sesión cargada - LoggedIn: {SessionService.IsLoggedIn}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando sesión: {ex.Message}");
            }
        }

        private async Task CheckForUpdatesOnStartup()
        {
            try
            {
                await Task.Delay(3000); // Espera más corta

                var update = await AppVersionManager.CheckForUpdatesAsync();

                if (update != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await ShowUpdateAlert(update);
                    });
                }
            }
            catch
            {
                // Silencioso
            }
        }

        private async Task ShowUpdateAlert(VersionInfo version)
        {
            string title = version.es_obligatoria ? "⚠️ ACTUALIZACIÓN OBLIGATORIA" : "📱 ACTUALIZACIÓN DISPONIBLE";

            if (version.es_obligatoria)
            {
                bool update = await Application.Current.MainPage.DisplayAlert(
                    title,
                    version.descripcion,
                    "Actualizar",
                    "Salir");

                if (update)
                {
                    await Launcher.OpenAsync(version.url_descarga);
                    Application.Current.Quit();
                }
                else
                {
                    Application.Current.Quit();
                }
            }
            else
            {
                bool update = await Application.Current.MainPage.DisplayAlert(
                    title,
                    version.descripcion,
                    "Actualizar",
                    "Más tarde");

                if (update)
                {
                    await Launcher.OpenAsync(version.url_descarga);
                }
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Forma CORRECTA para .NET 9.0 - Usar el service provider
            var splashPage = _serviceProvider.GetRequiredService<SplashPage>();

            return new Window(new NavigationPage(splashPage))
            {
                Title = "SRQCC Movil"
            };
        }
    }
}