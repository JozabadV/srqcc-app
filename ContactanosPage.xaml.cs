using Microsoft.Maui.ApplicationModel.Communication;

namespace SRQCC
{
    public partial class ContactanosPage : ContentPage
    {
        public ContactanosPage()
        {
            InitializeComponent();
        }

        // Copiar Teléfono al Portapapeles
        private async void OnCopiarTelefonoClicked(object sender, EventArgs e)
        {
            try
            {
                string telefono = "+51984703188";
                await Clipboard.Default.SetTextAsync(telefono);

                // Mostrar confirmación
                await DisplayAlert("✅ Copiado", "Número de teléfono copiado al portapapeles", "Aceptar");

                // Opcional: Vibración (solo Android)
#if ANDROID
                if (Vibration.Default.IsSupported)
                {
                    Vibration.Default.Vibrate(100);
                }
#endif
            }
            catch (Exception ex)
            {
                //await DisplayAlert("Error", $"No se pudo copiar el número: {ex.Message}", "Aceptar");
            }
        }

        // Copiar Correo al Portapapeles
        private async void OnCopiarCorreoClicked(object sender, EventArgs e)
        {
            try
            {
                string correo = "atencionalasociado@srq-cc.com";
                await Clipboard.Default.SetTextAsync(correo);

                // Mostrar confirmación
                await DisplayAlert("✅ Copiado", "Correo electrónico copiado al portapapeles", "Aceptar");

                // Opcional: Vibración (solo Android)
#if ANDROID
                if (Vibration.Default.IsSupported)
                {
                    Vibration.Default.Vibrate(100);
                }
#endif
            }
            catch (Exception ex)
            {
                //await DisplayAlert("Error", $"No se pudo copiar el correo: {ex.Message}", "Aceptar");
            }
        }


        // Abrir WhatsApp
        private async void OnAbrirWhatsAppClicked(object sender, EventArgs e)
        {
            try
            {
                string numeroWhatsApp = "51984703188"; // Número sin el +
                string mensaje = "Hola, necesito información sobre Santa Rosa de Quives Country Club";

                string url = $"https://wa.me/{numeroWhatsApp}?text={Uri.EscapeDataString(mensaje)}";

                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir WhatsApp: {ex.Message}", "Aceptar");
            }
        }



        // Abrir Google Maps
        private async void OnAbrirMapsClicked(object sender, EventArgs e)
        {
            try
            {
                //string direccion = "Santa+Rosa+de+Quives+Country+Club+Lima+Peru";
                //string url = $"https://www.google.com/maps/search/?api=1&query={direccion}";
                string url = $"https://maps.app.goo.gl/NCZb51htxsNRYTX48";

                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir Google Maps: {ex.Message}", "Aceptar");
            }
        }

        // Abrir Google Maps
        private async void OnAbrirMapsOficinaClicked(object sender, EventArgs e)
        {
            try
            {
                string direccion = "Av. Carlos Izaguirre 1153, Los Olivos";
                string url = $"https://www.google.com/maps/search/?api=1&query={direccion}";

                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir Google Maps: {ex.Message}", "Aceptar");
            }
        }

        // Facebook
        private async void OnFacebookClicked(object sender, EventArgs e)
        {
            try
            {
                string url = "https://www.facebook.com/srqcc/";
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir Facebook: {ex.Message}", "Aceptar");
            }
        }

        // Instagram
        private async void OnInstagramClicked(object sender, EventArgs e)
        {
            try
            {
                string url = "https://www.instagram.com/santarosadequives/";
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir Instagram: {ex.Message}", "Aceptar");
            }
        }

        // YouTube
        private async void OnYouTubeClicked(object sender, EventArgs e)
        {
            try
            {
                string url = "http://www.youtube.com/@ClubSantaRosaDeQuivesTV";
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir YouTube: {ex.Message}", "Aceptar");
            }
        }


        // TikTok
        private async void OnTikTokClicked(object sender, EventArgs e)
        {
            try
            {
                string url = "https://www.tiktok.com/@santarosadequives.cc";
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir YouTube: {ex.Message}", "Aceptar");
            }
        }

        // Linktree
        private async void OnLinkTreeClicked(object sender, EventArgs e)
        {
            try
            {
                string url = "https://linktr.ee/SantaRosadeQuivesCC";
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir YouTube: {ex.Message}", "Aceptar");
            }
        }

        // Manejar botón atrás
        protected override bool OnBackButtonPressed()
        {
            Dispatcher.Dispatch(async () =>
            {
                await Shell.Current.GoToAsync("//MainPage");
            });
            return true;
        }
    }
}