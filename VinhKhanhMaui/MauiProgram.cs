using Microsoft.Extensions.Logging;

namespace VinhKhanhMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if ANDROID
        builder.ConfigureMauiHandlers((Action<IMauiHandlersCollection>?)(handlers =>
        {
            MauiHandlersCollectionExtensions.AddHandler<Microsoft.Maui.Controls.Maps.Map,
                Platforms.Android.ColoredMapHandler>(handlers);
        }));
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}