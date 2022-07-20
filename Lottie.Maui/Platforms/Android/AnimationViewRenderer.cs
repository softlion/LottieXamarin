using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Com.Airbnb.Lottie;
using Android.Content;
using Lottie.Maui.Platforms.Android;
using Microsoft.Maui.Handlers;

namespace Lottie.Maui.Platforms;


internal class AnimationViewHandler : ViewHandler<IAnimationView, LottieAnimationView>
{
    private AnimatorListener _animatorListener;
    private AnimatorUpdateListener _animatorUpdateListener;
    private LottieOnCompositionLoadedListener _lottieOnCompositionLoadedListener;
    private LottieFailureListener _lottieFailureListener;
    private ClickListener _clickListener;
    
    private AnimationView RealVirtualView => (AnimationView)VirtualView;
    
    public static IPropertyMapper<IAnimationView, AnimationViewHandler> Mapper = new PropertyMapper<IAnimationView, AnimationViewHandler>(ViewMapper)
    {
        [nameof(IAnimationView.Animation)] = MapAnimation,
    };

    public static CommandMapper<IAnimationView, AnimationViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        //[nameof(IAnimationView.FireClicked)] = MapFireClicked
    };
    
    
    /// <summary>
    /// Required
    /// </summary>
    public AnimationViewHandler() : base(Mapper, CommandMapper)
    {
    }
    
    public AnimationViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null) : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override LottieAnimationView CreatePlatformView()
    {
        return new LottieAnimationView(Context);
    }

    protected override void ConnectHandler(LottieAnimationView animationView)
    {
        var element = RealVirtualView;
        
        _animatorListener = new AnimatorListener
        {
            OnAnimationCancelImpl = () => element.InvokeStopAnimation(),
            OnAnimationEndImpl = () => element.InvokeFinishedAnimation(),
            OnAnimationPauseImpl = () => element.InvokePauseAnimation(),
            OnAnimationRepeatImpl = () => element.InvokeRepeatAnimation(),
            OnAnimationResumeImpl = () => element.InvokeResumeAnimation(),
            OnAnimationStartImpl = () => element.InvokePlayAnimation()
        };
        _animatorUpdateListener = new() { OnAnimationUpdateImpl = (progress) => element.InvokeAnimationUpdate(progress) };
        _lottieOnCompositionLoadedListener = new() { OnCompositionLoadedImpl = (composition) => element.InvokeAnimationLoaded(composition) };
        _lottieFailureListener = new() { OnResultImpl = (exception) => element.InvokeFailure(exception) };
        _clickListener = new() { OnClickImpl = () => element.InvokeClick() };

        animationView.AddAnimatorListener(_animatorListener);
        animationView.AddAnimatorUpdateListener(_animatorUpdateListener);
        animationView.AddLottieOnCompositionLoadedListener(_lottieOnCompositionLoadedListener);
        animationView.SetFailureListener(_lottieFailureListener);
        animationView.SetOnClickListener(_clickListener);

        animationView.TrySetAnimation(element);

        element.PlayCommand = new Command(() => animationView.PlayAnimation());
        element.PauseCommand = new Command(() => animationView.PauseAnimation());
        element.ResumeCommand = new Command(() => animationView.ResumeAnimation());
        element.StopCommand = new Command(() =>
        {
            animationView.CancelAnimation();
            animationView.Progress = 0.0f;
        });
        element.ClickCommand = new Command(() => animationView.PerformClick());

        element.PlayMinAndMaxFrameCommand = new Command((object paramter) =>
        {
            if (paramter is (int minFrame, int maxFrame))
            {
                animationView.SetMinAndMaxFrame(minFrame, maxFrame);
                animationView.PlayAnimation();
            }
        });
        element.PlayMinAndMaxProgressCommand = new Command((object paramter) =>
        {
            if (paramter is (float minProgress, float maxProgress))
            {
                animationView.SetMinAndMaxProgress(minProgress, maxProgress);
                animationView.PlayAnimation();
            }
        });
        element.ReverseAnimationSpeedCommand = new Command(() => animationView.ReverseAnimationSpeed());

        animationView.SetCacheComposition(element.CacheComposition);
        //_animationView.SetFallbackResource(element.FallbackResource.);
        //_animationView.Composition = element.Composition;

        if (element.MinFrame != int.MinValue)
            animationView.SetMinFrame(element.MinFrame);
        if (element.MinProgress != float.MinValue)
            animationView.SetMinProgress(element.MinProgress);
        if (element.MaxFrame != int.MinValue)
            animationView.SetMaxFrame(element.MaxFrame);
        if (element.MaxProgress != float.MinValue)
            animationView.SetMaxProgress(element.MaxProgress);

        animationView.Speed = element.Speed;

        animationView.ConfigureRepeat(element.RepeatMode, element.RepeatCount);

        if (!string.IsNullOrEmpty(element.ImageAssetsFolder))
            animationView.ImageAssetsFolder = element.ImageAssetsFolder;

        animationView.Frame = element.Frame;
        animationView.Progress = element.Progress;

        animationView.EnableMergePathsForKitKatAndAbove(element.EnableMergePathsForKitKatAndAbove);

        if (element.AutoPlay || element.IsAnimating)
            animationView.PlayAnimation();

        element.Duration = animationView.Duration;
        element.IsAnimating = animationView.IsAnimating;

        base.ConnectHandler(animationView);
    }

    protected override void DisconnectHandler(LottieAnimationView animationView)
    {
        animationView.RemoveAnimatorListener(_animatorListener);
        animationView.RemoveAllUpdateListeners();
        animationView.RemoveLottieOnCompositionLoadedListener(_lottieOnCompositionLoadedListener);
        animationView.SetFailureListener(null);
        animationView.SetOnClickListener(null);

        base.DisconnectHandler(animationView);
    }
    
    static void MapAnimation(AnimationViewHandler handler, IAnimationView item)
    {
        var element = handler.RealVirtualView;
        handler.PlatformView.TrySetAnimation(element);
        if (element.AutoPlay || element.IsAnimating)
            handler.PlatformView.PlayAnimation();
    }
    
    /*
        if (e.PropertyName == AnimationView.CacheCompositionProperty.PropertyName)
            _animationView.SetCacheComposition(Element.CacheComposition);

        if (e.PropertyName == AnimationView.EnableMergePathsForKitKatAndAboveProperty.PropertyName)
            _animationView.EnableMergePathsForKitKatAndAbove(Element.EnableMergePathsForKitKatAndAbove);

        if (e.PropertyName == AnimationView.MinFrameProperty.PropertyName)
            _animationView.SetMinFrame(Element.MinFrame);

        if (e.PropertyName == AnimationView.MinProgressProperty.PropertyName)
            _animationView.SetMinProgress(Element.MinProgress);

        if (e.PropertyName == AnimationView.MaxFrameProperty.PropertyName)
            _animationView.SetMaxFrame(Element.MaxFrame);

        if (e.PropertyName == AnimationView.MaxProgressProperty.PropertyName)
            _animationView.SetMaxProgress(Element.MaxProgress);

        if (e.PropertyName == AnimationView.SpeedProperty.PropertyName)
            _animationView.Speed = Element.Speed;

        if (e.PropertyName == AnimationView.RepeatModeProperty.PropertyName || e.PropertyName == AnimationView.RepeatCountProperty.PropertyName)
            _animationView.ConfigureRepeat(Element.RepeatMode, Element.RepeatCount);

        if (e.PropertyName == AnimationView.ImageAssetsFolderProperty.PropertyName && !string.IsNullOrEmpty(Element.ImageAssetsFolder))
            _animationView.ImageAssetsFolder = Element.ImageAssetsFolder;

        if (e.PropertyName == AnimationView.FrameProperty.PropertyName)
            _animationView.Frame = Element.Frame;

        if (e.PropertyName == AnimationView.ProgressProperty.PropertyName)
            _animationView.Progress = Element.Progress;
     */
}

