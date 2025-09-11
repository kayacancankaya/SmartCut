using Microsoft.Extensions.Logging;
using SmartCut.Services;
using SmartCut.Shared.Services;
using System.Globalization;

namespace SmartCut
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
                });

            // Add device-specific services used by the SmartCut.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            builder.Services.AddScoped(sp =>
            {
                Uri? baseAddress = null;

                if (DeviceInfo.Platform == DevicePlatform.Android)
                    baseAddress = new Uri("https://admin.birilerigt.com/"); // emulator localhost workaround
                else if (DeviceInfo.Platform == DevicePlatform.iOS)
                    baseAddress = new Uri("http://localhost:5001/"); // iOS simulator localhost workaround
                else if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
                    baseAddress = new Uri("http://localhost:5001/"); // Mac Catalyst localhost workaround
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    baseAddress = new Uri("http://localhost:7110/");
                else
                    baseAddress = new Uri("https://smartcut.smartifie.com/");

                return new HttpClient { BaseAddress = baseAddress };
            });
            builder.Services.AddScoped<ApiClient>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<BreadcrumbService>();
     



            // 1. Set default culture to en-US
            var defaultCulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = defaultCulture;
            CultureInfo.CurrentUICulture = defaultCulture;

            // 2. Try to get device culture
#if ANDROID || IOS || MACCATALYST
            var deviceCulture = System.Globalization.CultureInfo.CurrentCulture;
#else
            var deviceCulture = CultureInfo.InstalledUICulture; // fallback for Windows
#endif

            // 3. If device culture is supported, set it globally
            if (deviceCulture != null)
            {
                CultureInfo.CurrentCulture = deviceCulture;
                CultureInfo.CurrentUICulture = deviceCulture;
            }

            //temporary force to Turkish for testing
            //CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            //CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

            //temporary force to Spanish for testing
            //CultureInfo.CurrentCulture = new CultureInfo("es-ES");
            //CultureInfo.CurrentUICulture = new CultureInfo("es-ES");

            // Apply globally for all threads
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;

            return builder.Build();

        }

    }
}

