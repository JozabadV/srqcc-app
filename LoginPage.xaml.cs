using SRQCC.Services;

namespace SRQCC
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var password = PasswordEntry.Text;
            var user = UsernameEntry.Text;


            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Por favor complete todos los campos", "Aceptar");
                return;
            }

            LoginButton.IsEnabled = false;
            LoginButton.Text = "Validando...";

            try
            {
                // Crear instancia directamente
                var databaseService = new DatabaseService();
                var isValid = await databaseService.ValidatePasswordAsync(user, password);

                if (isValid)
                {
                    // Obtener el nombre completo del asociado
                    var nombreCompleto = await databaseService.GetNombreCompletoAsync(user);

                    // Obtener el estado de pago
                    var estadoPago = await databaseService.GetEstadoPagoAsync(user);

                    // Obtener Dni
                    var codigo_asociado = await databaseService.GetCodigoAsociadoAsync(user);

                    //  GUARDAR EN SESIÓN (con el nuevo método que incluye persistencia)
                    SessionService.Login(codigo_asociado, nombreCompleto, estadoPago, user);

                    // NAVEGAR A LA APP PRINCIPAL
                    if (Application.Current != null && Application.Current.Windows.Count > 0)
                    {
                        Application.Current.Windows[0].Page = new AppShell();
                    }

                    //Console.WriteLine($"Login exitoso: {nombreCompleto}");
                }
                else
                {
                    bool result = await DisplayAlert(
                        "Error",
                        "No se ha podido iniciar sesión. Sus credenciales son incorrectas o aún no ha creado una contraseña. ¿Desea ir a la página de creación de contraseña?",
                        "Sí, Abrir",
                        "Cancelar"
                    );
                    if (result)
                    {
                        try
                        {
                            var url = "https://srqcc.pe/Auth/Index";
                            await Launcher.OpenAsync(url);
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("Error", $"No se pudo abrir el enlace: {ex.Message}", "Aceptar");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error: {ex.Message}", "Aceptar");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Text = "Iniciar sesión";
            }
        }

        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;

            var button = (Button)sender;
            var imageSource = (FileImageSource)button.ImageSource;

            // Cambiar la imagen según el estado
            if (PasswordEntry.IsPassword)
            {
                imageSource.File = "eye_closed.png";
            }
            else
            {
                imageSource.File = "eye_open.png";
            }
        }

        // Opcional: Método para manejar el olvido de contraseña
        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            bool result = await DisplayAlert(
                "Recuperar Contraseña",
                "¿Desea abrir la página de recuperación de contraseña en su navegador?",
                "Sí, Abrir",
                "Cancelar"
            );

            if (result)
            {
                try
                {
                    var url = "https://srqcc.pe/Auth/Index";
                    await Launcher.OpenAsync(url);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo abrir el enlace: {ex.Message}", "Aceptar");
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            NavigationPage.SetHasNavigationBar(this, false);

            // Limpiar campos al aparecer
            UsernameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;

            // VERIFICAR SI YA HAY UNA SESIÓN ACTIVA (por si acaso)
            if (SessionService.IsLoggedIn)
            {
                Console.WriteLine($"🔍 Sesión activa detectada en LoginPage: {SessionService.NombreCompleto}");
                // Redirigir automáticamente :
                // Application.Current.Windows[0].Page = new AppShell();
            }
        }
    }
}