using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;
using SRQCC.Services;
using System.Timers;
using static SRQCC.Services.DatabaseService;

namespace SRQCC
{
    public partial class MainPage : ContentPage
    {
        private readonly QrCodeService _qrService;
        private readonly DatabaseService _databaseService;
        private System.Timers.Timer _timer; // Especificar el namespace completo

        protected override bool OnBackButtonPressed()
        {
            // En la página principal, dejar que la app se cierre
            //Console.WriteLine(" App se cierra desde MainPage");
            return false; // false = permitir cierre
        }

        public MainPage()
        {
            InitializeComponent();

            _qrService = new QrCodeService();
            _databaseService = new DatabaseService();
            
            //  INICIAR TIMER PARA LA HORA
            IniciarTimerFechaHora();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            LoadUserData();
            GenerarQrCode();
            await LoadUserPhoto(SessionService.Dni);
            //  ACTUALIZAR HORA INMEDIATAMENTE
            ActualizarFechaHora();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //  DETENER TIMER AL SALIR DE LA PÁGINA
            _timer?.Stop();
            _timer?.Dispose();
        }

        //  MÉTODO PARA INICIAR EL TIMER
        private void IniciarTimerFechaHora()
        {
            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                ActualizarFechaHora();
                return true; // Continuar ejecutando
            });
        }

        //  MÉTODO PARA ACTUALIZAR FECHA Y HORA
        private void ActualizarFechaHora()
        {
            try
            {
                var ahora = DateTime.Now;

                // Formatear fecha en español (más eficiente con CultureInfo)
                var culture = new System.Globalization.CultureInfo("es-ES");
                FechaLabel.Text = ahora.ToString("dddd, dd 'de' MMMM 'de' yyyy", culture);

                // Formatear hora - asegurar que siempre muestre 2 dígitos
                HoraLabel.Text = ahora.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error actualizando fecha y hora: {ex.Message}");
            }
        }

        private void LoadUserData()
        {
            if (SessionService.IsLoggedIn)
            {
                NombreLabel.Text = SessionService.NombreCompleto.ToUpper();
                CodigoLabel.Text = $"Código: {SessionService.CodigoAsociado}";
            }
            else
            {
                NombreLabel.Text = "USUARIO NO AUTENTICADO";
                CodigoLabel.Text = "Código: N/A";
                UserPhotoImage.Source = "user_placeholder.png";
            }
        }

        private async Task LoadUserPhoto(string dni)
        {
            try
            {
                // Usar DatabaseService para obtener el código de imagen
                string codigoImagen = await _databaseService.GetCodigoImagenAsync(SessionService.Dni);

                

                if (!string.IsNullOrEmpty(codigoImagen))
                {
                    // Construir la URL de la imagen
                    string urlImagen = $"https://documentos.srqcc.pe/imagenes/asociados_foto/{codigoImagen}.jpg";
                    

                    // Verificar si la imagen existe
                    bool imagenExiste = await VerificarImagenExiste(urlImagen);
                    

                    // Actualizar en el hilo principal
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
                //Console.WriteLine($" Error loading user photo: {ex.Message}");
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
                ;
                return false;
            }
        }

        private async void OnPerfilTapped(object sender, EventArgs e)
        {
            if (!SessionService.IsLoggedIn)
            {
                await DisplayAlert("Error", "Debe iniciar sesión primero", "Aceptar");
                return;
            }

            await Shell.Current.GoToAsync("//MiPerfil");
        }

        private async void OnPagosTapped(object sender, EventArgs e)
        {
            if (!SessionService.IsLoggedIn)
            {
                await DisplayAlert("Error", "Debe iniciar sesión primero", "Aceptar");
                return;
            }

            await Shell.Current.GoToAsync("//MisPagos");
        }

        private void GenerarQrCode()
        {
            if (!SessionService.IsLoggedIn)
            {
                QrImage.Source = null;
                return;
            }

            try
            {
                string dni = SessionService.Dni;
                string dniEncrypted = EncryptionHelper.Encrypt(dni);
                string urlControl = $"http://control.srqcc.pe/asociado/{dniEncrypted}";
                var qrImageSource = _qrService.GenerateQrCodeSimple(urlControl);

                if (qrImageSource == null)
                {
                    //Console.WriteLine(" QR source es NULL");
                    QrImage.Source = null;
                }
                else
                {
                    //Console.WriteLine(" QR generado exitosamente");
                    QrImage.Source = qrImageSource;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($" Error generando QR: {ex.Message}");
                QrImage.Source = null;
            }
        }
    }
}