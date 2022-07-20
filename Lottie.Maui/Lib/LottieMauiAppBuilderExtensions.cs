using Lottie.Maui;
using Lottie.Maui.Platforms;

namespace Lottie;

public static class LottieMauiAppBuilderExtensions
{
    /// <summary>
    /// Add Maui handlers
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MauiAppBuilder AddLottie(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
#if ANDROID
            handlers.TryAddHandler<AnimationView,AnimationViewHandler>();
#elif IOS || MACCATALYST
            handlers.TryAddHandler<AnimationView, AnimationViewHandler>();
#elif WINDOWS
            //handlers.TryAddHandler<AnimationView,AnimationViewHandler>();
#endif
        });

        return builder;
    }
}
