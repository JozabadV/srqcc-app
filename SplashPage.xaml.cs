using SRQCC.Services;


namespace SRQCC
{
    public partial class SplashPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public SplashPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            NavigateToAppropriatePage();
        }
        
        private async void NavigateToAppropriatePage()
        {
            await Task.Delay(2000);

            // VERIFICAR SI HAY SESIÓN ACTIVA
            if (SessionService.IsLoggedIn)
            {
                await NavigateToAppShell();
            }
            else
            {
                await NavigateToLogin();
            }
        }

        private async Task NavigateToAppShell()
        {
            // Si hay sesión, ir directamente a la app principal
            if (Application.Current?.Windows?.Count > 0)
            {
                Application.Current.Windows[0].Page = new AppShell();
            }
            else
            {
                // Fallback por si acaso
                await NavigateToLogin();
            }
        }

        private async Task NavigateToLogin()
        {
            // Usar el service provider para crear LoginPage con sus dependencias
            var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
            await Navigation.PushAsync(loginPage);

            Navigation.RemovePage(this);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}