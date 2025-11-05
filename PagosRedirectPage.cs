using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace SRQCC
{
    public partial class PagosRedirectPage : ContentPage
    {
        private bool _linkAbierto = false;

        public PagosRedirectPage()
        {
            // Página completamente invisible
            BackgroundColor = Colors.Transparent;
            Opacity = 0;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_linkAbierto) return;
            _linkAbierto = true;

            // No esperar - hacer todo en background
            _ = Task.Run(async () =>
            {
                await Launcher.OpenAsync("https://srqcc.pe/web/pagossrq.html");
            });

            // Regresar INMEDIATAMENTE
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}