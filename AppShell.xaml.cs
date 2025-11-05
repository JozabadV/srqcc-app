using SRQCC;
using SRQCC.Services;
using Microsoft.Maui.ApplicationModel;



namespace SRQCC
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            BindingContext = this;
            // Registrar las rutas para navegación
            RegisterRoutes();
        }
        public string AppVersion => $"Versión: {AppVersionManager.CurrentVersion}";
        private void RegisterRoutes()
        {
            // Registrar solo las páginas que existen
            try
            {
                Routing.RegisterRoute("MainPage", typeof(MainPage));
                Routing.RegisterRoute("MiPerfil", typeof(MiPerfil));
                Routing.RegisterRoute("MisPagos", typeof(MisPagos));
                Routing.RegisterRoute("ContactanosPage", typeof(ContactanosPage));
                Routing.RegisterRoute("PagosRedirect", typeof(PagosRedirectPage));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registrando rutas: {ex.Message}");
            }
        }

        // Nuevo método para cargar información del usuario
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        //  Evento para cerrar sesión
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Cerrar Sesión",
                "¿Está seguro que desea cerrar sesión?", "Sí", "No");

            if (confirm)
            {
                SessionService.Logout();

                if (Application.Current?.Windows?.Count > 0)
                {
                    Application.Current.Windows[0].Page = new LoginPage();
                }
            }
        }
    }
}