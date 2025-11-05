using FFImageLoading.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using SRQCC.Services;

namespace SRQCC
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            //  Registrar el servicio de base de datos
            builder.Services.AddSingleton<DatabaseService>();

            // Registrar las páginas CON sus dependencias
            
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SplashPage>();
            builder.Services.AddTransient<MiPerfil>();
            builder.Services.AddTransient<MisPagos>();
            builder.Services.AddTransient<ContactanosPage>();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<PagosRedirectPage>();
            builder.UseFFImageLoading();
            return builder.Build();
        }
    }
}