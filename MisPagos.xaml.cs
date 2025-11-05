using Microsoft.Maui.Controls.Shapes;
using SRQCC.Models;
using SRQCC.Services;
using System.Globalization;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace SRQCC
{
    public partial class MisPagos : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public MisPagos()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ActualizarEstadoPago();
            ActualizarInformacionAdicional();
            CargarAnios();
            CargarHistorialActual();
            // CLICK AUTOMÁTICO DIRECTO
            if (ActualizarButton?.IsEnabled == true)
            {
                Dispatcher.Dispatch(async () =>
                {
                    await Task.Delay(50); // Pequeño delay para UI
                    OnActualizarEstadoClicked(ActualizarButton, EventArgs.Empty);
                });
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // En MiPerfil, regresar a MainPage
            //Console.WriteLine("↩️ Regresando a MainPage desde MisPagos");

            //  FORMA CORRECTA en .NET MAUI (sin Device)
            Dispatcher.Dispatch(async () =>
            {
                await Shell.Current.GoToAsync("//MainPage");
            });

            return true; // true = nosotros manejamos el evento
        }


        private void ActualizarEstadoPago()
        {
            if (!SessionService.IsLoggedIn)
            {
                SetEstadoNoAutenticado();
                return;
            }

            // VERIFICAR SI ES ASOCIADO INACTIVO
            var estadoAsociado = SessionService.EstadoPago; // Ahora 0 = INACTIVO

            if (estadoAsociado == 0) // ASOCIADO INACTIVO
            {
                SetEstadoInactivo();
                return;
            }

            switch (estadoAsociado)
            {
                case 2: // Al día
                    SetEstadoAlDia();
                    break;

                case -1: // Sin registro
                    SetEstadoSinRegistro();
                    break;

                default: // Moroso u otro estado
                    SetEstadoPendiente();
                    break;
            }
        }

        private void ActualizarInformacionAdicional()
        {
            FechaConsultaLabel.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
        }

        private void SetEstadoInactivo()
        {
            EstadoCircle.BackgroundColor = Color.FromArgb("#9CA3AF");
            EstadoIcon.Source = "icon_user.png";
            EstadoLabel.Text = "ASOCIADO INACTIVO";
            EstadoLabel.TextColor = Color.FromArgb("#6B7280");
            DetalleLabel.Text = "Asociación suspendida o finalizada";
            DetalleLabel.TextColor = Color.FromArgb("#9CA3AF");
            EstadoPagoBorder.BackgroundColor = Color.FromArgb("#F9FAFB");
            EstadoActualLabel.Text = "ASOCIADO INACTIVO ⏸️";
            EstadoActualLabel.TextColor = Color.FromArgb("#6B7280");
        }

        private void SetEstadoAlDia()
        {
            EstadoCircle.BackgroundColor = Color.FromArgb("#48BB78");
            EstadoIcon.Source = "icon_check.png";
            EstadoLabel.Text = "AL DÍA";
            EstadoLabel.TextColor = Color.FromArgb("#2F855A");
            DetalleLabel.Text = "Pagos al corriente";
            DetalleLabel.TextColor = Color.FromArgb("#68D391");
            EstadoPagoBorder.BackgroundColor = Color.FromArgb("#F0FFF4");
            EstadoActualLabel.Text = "AL DÍA ✅";
            EstadoActualLabel.TextColor = Color.FromArgb("#2F855A");
        }

        public async void SetEstadoPendiente()
        {

            var primerMesPendiente = await _databaseService.ObtenerPrimerMesPendienteAsync(SessionService.Dni);
            string mes = primerMesPendiente?.NombreMes ?? "Mes no disponible";
            int año = primerMesPendiente.Anio;
            EstadoCircle.BackgroundColor = Color.FromArgb("#F56565");
            EstadoIcon.Source = "icon_warning.png";
            EstadoLabel.Text = "PENDIENTE";
            EstadoLabel.TextColor = Color.FromArgb("#C53030");
            DetalleLabel.Text = $"Pagos por regulizar desde {mes} de {año}";
            DetalleLabel.TextColor = Color.FromArgb("#FC8181");
            EstadoPagoBorder.BackgroundColor = Color.FromArgb("#FFF5F5");
            EstadoActualLabel.Text = "PENDIENTE ⚠️";
            EstadoActualLabel.TextColor = Color.FromArgb("#C53030");

        }

        private void SetEstadoSinRegistro()
        {
            EstadoCircle.BackgroundColor = Color.FromArgb("#ED8936");
            EstadoIcon.Source = "icon_info.png";
            EstadoLabel.Text = "SIN REGISTRO";
            EstadoLabel.TextColor = Color.FromArgb("#DD6B20");
            DetalleLabel.Text = "No hay registro de pago para el mes actual";
            DetalleLabel.TextColor = Color.FromArgb("#F6AD55");
            EstadoPagoBorder.BackgroundColor = Color.FromArgb("#FFFAF0");
            EstadoActualLabel.Text = "SIN REGISTRO ❓";
            EstadoActualLabel.TextColor = Color.FromArgb("#DD6B20");
        }

        private void SetEstadoNoAutenticado()
        {
            EstadoCircle.BackgroundColor = Color.FromArgb("#A0AEC0");
            EstadoIcon.Source = "icon_user.png";
            EstadoLabel.Text = "NO AUTENTICADO";
            EstadoLabel.TextColor = Color.FromArgb("#4A5568");
            DetalleLabel.Text = "Inicie sesión para ver el estado de pago";
            DetalleLabel.TextColor = Color.FromArgb("#CBD5E0");
            EstadoPagoBorder.BackgroundColor = Color.FromArgb("#F7FAFC");
            EstadoActualLabel.Text = "NO AUTENTICADO";
            EstadoActualLabel.TextColor = Color.FromArgb("#A0AEC0");
        }

        private async void OnActualizarEstadoClicked(object sender, EventArgs e)
        {
            if (!SessionService.IsLoggedIn)
            {
                await DisplayAlert("Información", "Debe iniciar sesión primero", "Aceptar");
                return;
            }

            try
            {
                EstadoLabel.Text = "Actualizando...";
                EstadoIcon.Source = "icon_loading.png";

                //  CORRECTO: Usar _databaseService que ya está instanciado
                var nuevoEstado = await _databaseService.GetEstadoPagoAsync(SessionService.Dni);

                SessionService.ActualizarEstadoPago(nuevoEstado);
                ActualizarEstadoPago();

                //await DisplayAlert("Actualizado", "Estado de pago actualizado", "Aceptar");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo actualizar: {ex.Message}", "Aceptar");
            }
        }

        private async void CargarAnios()
        {
            if (!SessionService.IsLoggedIn) return;

            try
            {
                //  OBTENER PRIMER AÑO DE PAGO DEL ASOCIADO
                var primerAnio = await _databaseService.GetPrimerAnioPagoAsync(SessionService.Dni);
                var anioActual = DateTime.Now.Year;
                var anios = new List<int>();

                // Años desde el primer pago hasta 2026
                for (int i = primerAnio; i <= 2026; i++)
                {
                    anios.Add(i);
                }

                AnioPicker.ItemsSource = anios;

                // Seleccionar el año actual, o si no existe, el último disponible
                if (anios.Contains(anioActual))
                {
                    AnioPicker.SelectedItem = anioActual;
                }
                else if (anios.Count > 0)
                {
                    AnioPicker.SelectedItem = anios.Last();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando años: {ex.Message}");
                // Fallback: años desde 2009
                var anios = new List<int>();
                for (int i = 2009; i <= 2026; i++) anios.Add(i);
                AnioPicker.ItemsSource = anios;
                AnioPicker.SelectedItem = DateTime.Now.Year;
            }
        }

        private async void CargarHistorialActual()
        {
            if (!SessionService.IsLoggedIn) return;

            if (AnioPicker.SelectedItem != null)
            {
                int anioSeleccionado = (int)AnioPicker.SelectedItem;
                await CargarHistorial(anioSeleccionado);
            }
            else
            {
                // Si no hay año seleccionado, cargar el año actual
                AnioPicker.SelectedItem = DateTime.Now.Year;
                await CargarHistorial(DateTime.Now.Year);
            }
        }

        private async void OnBuscarHistorialClicked(object sender, EventArgs e)
        {
            if (AnioPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Seleccione un año", "Aceptar");
                return;
            }

            int anioSeleccionado = (int)AnioPicker.SelectedItem;
            await CargarHistorial(anioSeleccionado);
        }

        private async Task CargarHistorial(int anio)
        {
            try
            {
                if (!SessionService.IsLoggedIn)
                {
                    await DisplayAlert("Error", "No hay sesión activa", "Aceptar");
                    return;
                }

                var dni = SessionService.Dni;

                // Obtener historial real de la BD
                var historial = await _databaseService.GetHistorialPagosAsync(dni, anio);

                // USAR LA VERSIÓN ASYNC
                var todosLosMeses = await GenerarMesesCompletos(anio, historial);

                MesesContainer.Children.Clear();

                foreach (var pago in todosLosMeses)
                {
                    MesesContainer.Children.Add(CrearCajaMes(pago));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar historial: {ex.Message}", "Aceptar");
            }
        }

        private async Task<List<PagoDetalle>> GenerarMesesCompletos(int anio, List<PagoDetalle> historialReal)
        {
            var todosLosMeses = new List<PagoDetalle>();

            //  OBTENER PRIMER MES DE PAGO PARA ESTE AÑO
            var primerMes = 1; // Por defecto empezar en enero
            var primerAnio = await _databaseService.GetPrimerAnioPagoAsync(SessionService.Dni);

            // Si es el primer año, obtener el primer mes de pago
            if (anio == primerAnio)
            {
                primerMes = await _databaseService.GetPrimerMesPagoAsync(SessionService.Dni, anio);
            }

            //  PARA ASOCIADOS INACTIVOS: solo mostrar hasta su último año de pago
            var esInactivo = SessionService.EstadoPago == 0;
            var anioActual = DateTime.Now.Year;
            var mesActual = DateTime.Now.Month;

            for (int mes = primerMes; mes <= 12; mes++) // Empezar desde el primer mes
            {
                //  PARA INACTIVOS: no mostrar meses futuros después de su último pago
                if (esInactivo && (anio > anioActual || (anio == anioActual && mes > mesActual)))
                {
                    continue; // Saltar meses futuros para inactivos
                }

                var pagoExistente = historialReal?.FirstOrDefault(p => p.mes == mes);

                if (pagoExistente != null)
                {
                    todosLosMeses.Add(pagoExistente);
                }
                else
                {
                    var fechaActual = DateTime.Now;
                    var fechaPago = new DateTime(anio, mes, 15);
                    int estado;

                    if (fechaPago > fechaActual)
                        estado = 1; // Pendiente
                    else
                        estado = 0; // Vencido

                    todosLosMeses.Add(new PagoDetalle
                    {
                        mes = mes,
                        anio = anio,
                        monto = 0.00m,
                        id_estado_pago_detalle = estado
                    });
                }
            }
            return todosLosMeses;
        }

        private Border CrearCajaMes(PagoDetalle pago)
        {
            // Determinar el texto del monto
            string textoMonto = pago.monto == 0.00m ? "SIN REGISTRO" : $"S/{pago.monto:N2}";

            // Crear el Border de estado primero (reemplazo de Frame)
            var estadoBorder = new Border
            {
                BackgroundColor = Color.FromArgb(pago.ColorEstado),
                StrokeShape = new RoundRectangle { CornerRadius = 15 },
                Padding = new Thickness(10, 5),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = pago.EstadoDisplay,
                    TextColor = Colors.White,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold
                }
            };

            //  Crear el Grid
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto }
        }
            };

            //  Columna izquierda - Información
            var leftContent = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
        {
            new Label
            {
                Text = "MANTENIMIENTO",
                FontSize = 14,
                TextColor = Color.FromArgb("#718096"),
                FontAttributes = FontAttributes.Bold
            },
            new Label
            {
                Text = textoMonto,
                FontSize = 16,
                TextColor = pago.monto == 0.00m ? Color.FromArgb("#A0AEC0") : Color.FromArgb(pago.ColorEstado),
                FontAttributes = FontAttributes.Bold
            },
            new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Color.FromArgb("#E2E8F0"),
                Margin = new Thickness(0, 5, 0, 5)
            },
            new Label
            {
                Text = $"Vencimiento: {pago.FechaVencimiento}",
                FontSize = 12,
                TextColor = Color.FromArgb("#718096")
            }
        }
            };

            //  Columna derecha - Estado
            Grid.SetColumn(estadoBorder, 1);

            //  Agregar elementos al Grid
            grid.Children.Add(leftContent);
            grid.Children.Add(estadoBorder);

            return new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(15),
                HorizontalOptions = LayoutOptions.Fill,
                Content = grid
            };
        }

        private async void OnAbrirPagosEnLineaClicked(object sender, EventArgs e)
        {
            try
            {

                string url = $"https://srqcc.pe/web/pagossrq.html";

                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo Pagos en linea: {ex.Message}", "Aceptar");
            }
        }
    }
}