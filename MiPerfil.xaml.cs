using Microsoft.Maui.Controls;
using SRQCC.Models;
using SRQCC.Services;


namespace SRQCC
{
    public partial class MiPerfil : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public MiPerfil()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override bool OnBackButtonPressed()
        {
            // En MiPerfil, regresar a MainPage
            //Console.WriteLine("↩️ Regresando a MainPage desde MiPerfil");

            //  FORMA CORRECTA en .NET MAUI (sin Device)
            Dispatcher.Dispatch(async () =>
            {
                await Shell.Current.GoToAsync("//MainPage");
            });

            return true; // true = nosotros manejamos el evento
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarPerfil();
        }

        private async Task CargarPerfil()
        {
            if (!SessionService.IsLoggedIn)
            {
                await DisplayAlert("Error", "No hay sesión activa", "Aceptar");
                return;
            }

            try
            {
                var dni = SessionService.Dni;
                //Console.WriteLine($" Buscando perfil para código: {codigoAsociado}");

                var perfil = await _databaseService.GetPerfilAsociadoAsync(dni);

                // Actualizar UI
                CodigoLabel.Text = SessionService.CodigoAsociado;
                NombreLabel.Text = perfil.NombreCompleto.ToUpper();
                NombresLabel.Text = perfil.Nombres;
                ApellidoPaternoLabel.Text = perfil.ApellidoPaterno;
                ApellidoMaternoLabel.Text = perfil.ApellidoMaterno;
                DocumentoLabel.Text = $"{perfil.TipoDocumento}: {SessionService.Dni}";
                TelefonoLabel.Text = perfil.Telefono;
                EmailLabel.Text = perfil.Email;
                EstadoLabel.Text = perfil.Estado;

                //Console.WriteLine($" Perfil cargado: {perfil.NombreCompleto}");

                //  CARGAR FOTO DEL USUARIO
                await CargarFotoUsuario();

                //  CARGAR DEPENDIENTES
                await CargarDependientes();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar perfil: {ex.Message}", "Aceptar");
                //Console.WriteLine($" Error: {ex.Message}");
            }
        }

        //  NUEVO: Método para abrir formulario de actualización de información
        private async void OnActualizarInfoTapped(object sender, EventArgs e)
        {
            try
            {
                string url = "https://forms.office.com/Pages/ResponsePage.aspx?id=vQ0QvMaT-UubEYtF3nXSBV7ox39yW2REpIZW6M-veOVUMVRQV1k2V0k2VEpHN1c0VjEzMDhBNEQ5WSQlQCN0PWcu";
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir el formulario: {ex.Message}", "Aceptar");
            }
        }

        //  NUEVO: Método para abrir formulario de actualización de dependientes
        private async void OnActualizarDependientesTapped(object sender, EventArgs e)
        {
            try
            {
                string url = "https://forms.office.com/r/JNFctW0qtV";
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir el formulario: {ex.Message}", "Aceptar");
            }
        }

        private async Task CargarFotoUsuario()
        {
            try
            {
                //Console.WriteLine($" Buscando foto para DNI: {SessionService.Dni}");

                string codigoImagen = await _databaseService.GetCodigoImagenAsync(SessionService.Dni);
                //Console.WriteLine($" Código imagen obtenido: {codigoImagen}");

                if (!string.IsNullOrEmpty(codigoImagen))
                {
                    string urlImagen = $"https://documentos.srqcc.pe/imagenes/asociados_foto/{codigoImagen}.jpg";
                    //Console.WriteLine($" URL de imagen: {urlImagen}");

                    bool imagenExiste = await VerificarImagenExiste(urlImagen);
                    //Console.WriteLine($" Imagen existe: {imagenExiste}");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UserPhotoImage.Source = imagenExiste
                            ? ImageSource.FromUri(new Uri(urlImagen))
                            : "user_placeholder.png";
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UserPhotoImage.Source = "user_placeholder.png";
                    });
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($" Error cargando foto de usuario: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UserPhotoImage.Source = "user_placeholder.png";
                });
            }
        }

        private async Task<bool> VerificarImagenExiste(string urlImagen)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    var response = await httpClient.GetAsync(urlImagen);
                    bool existe = response.IsSuccessStatusCode;
                    return existe;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($" Error verificando imagen: {ex.Message}");
                return false;
            }
        }

        private async Task CargarDependientes()
        {
            try
            {
                var dependientes = await _databaseService.GetDependientesAsync(SessionService.Dni);

                if (dependientes != null && dependientes.Any())
                {
                    DependientesCollectionView.ItemsSource = dependientes;
                    DependientesCollectionView.IsVisible = true;
                    SinDependientesLabel.IsVisible = false;
                }
                else
                {
                    DependientesCollectionView.IsVisible = false;
                    SinDependientesLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($" Error cargando dependientes: {ex.Message}");
                DependientesCollectionView.IsVisible = false;
                SinDependientesLabel.IsVisible = true;
                SinDependientesLabel.Text = "Error al cargar dependientes";
            }
        }
    }
}