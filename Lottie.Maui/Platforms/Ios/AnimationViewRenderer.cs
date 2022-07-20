using Airbnb.Lottie;
using Foundation;
using UIKit;
using Microsoft.Maui.Handlers;

namespace Lottie.Maui.Platforms;

internal class AnimationViewHandler : ViewHandler<IAnimationView, LOTAnimationView>
{
    private UITapGestureRecognizer? gestureRecognizer;
    private int repeatCount = 1;

    private AnimationView RealVirtualView => (AnimationView)VirtualView;

    public static IPropertyMapper<IAnimationView, AnimationViewHandler> Mapper = new PropertyMapper<IAnimationView, AnimationViewHandler>(ViewMapper)
    {
        [nameof(IAnimationView.Animation)] = MapAnimation,
    };

    public static CommandMapper<IAnimationView, AnimationViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        //[nameof(IAnimationView.FireClicked)] = MapFireClicked
    };


    public AnimationViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null) : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override LOTAnimationView CreatePlatformView()
    {
        var animationView = new LOTAnimationView
        {
            AutoresizingMask = UIViewAutoresizing.All,
            ContentMode = UIViewContentMode.ScaleAspectFit,
            LoopAnimation = e.NewElement.RepeatMode == RepeatMode.Infinite,
            AnimationSpeed = e.NewElement.Speed,
            AnimationProgress = e.NewElement.Progress,
            CacheEnable = e.NewElement.CacheComposition,
            CompletionBlock = new LOTAnimationCompletionBlock(AnimationCompletionBlock)
        };

        return animationView;
    }

    protected override void ConnectHandler(LOTAnimationView animationView)
    {
        var element = RealVirtualView;
        
        element.PlayCommand = new Command(() =>
        {
            animationView.PlayWithCompletion(AnimationCompletionBlock);
            element.InvokePlayAnimation();
        });
        element.PauseCommand = new Command(() =>
        {
            animationView.Pause();
            element.InvokePauseAnimation();
        });
        element.ResumeCommand = new Command(() =>
        {
            animationView.PlayWithCompletion(AnimationCompletionBlock);
            element.InvokeResumeAnimation();
        });
        element.StopCommand = new Command(() =>
        {
            animationView.Stop();
            element.InvokeStopAnimation();
        });
        element.ClickCommand = new Command(() =>
        {
            //_animationView.Click();
            //element.InvokeClick();
        });

        element.PlayMinAndMaxFrameCommand = new Command(parameter =>
        {
            if (parameter is (int minFrame, int maxFrame))
                animationView.PlayFromFrame(NSNumber.FromInt32(minFrame), NSNumber.FromInt32(maxFrame), AnimationCompletionBlock);
        });
        element.PlayMinAndMaxProgressCommand = new Command((parameter) =>
        {
            if (parameter is (float minProgress, float maxProgress))
                animationView.PlayFromProgress(minProgress, maxProgress, AnimationCompletionBlock);
        });
        element.ReverseAnimationSpeedCommand = new Command(() => animationView.AutoReverseAnimation = !animationView.AutoReverseAnimation);

        animationView.CacheEnable = element.CacheComposition;
        animationView.AnimationSpeed = element.Speed;
        animationView.LoopAnimation = element.RepeatMode == RepeatMode.Infinite;
        animationView.AnimationProgress = element.Progress;
        gestureRecognizer = new (element.InvokeClick);
        animationView.AddGestureRecognizer(gestureRecognizer);

        base.ConnectHandler(animationView);
    }

    protected override void DisconnectHandler(LOTAnimationView animationView)
    {
        repeatCount = 1;

        if (gestureRecognizer != null)
        {
            animationView.RemoveGestureRecognizer(gestureRecognizer);
            gestureRecognizer.Dispose();
            gestureRecognizer = null;
        }

        base.DisconnectHandler(animationView);
    }
    
    static void MapAnimation(AnimationViewHandler handler, IAnimationView item)
    {
        var element = handler.RealVirtualView;
        var composition = element.GetAnimation();
        handler.PlatformView.SceneModel = composition;
        element.InvokeAnimationLoaded(composition);
        if (element.AutoPlay || element.IsAnimating)
            handler.PlatformView.PlayWithCompletion(handler.AnimationCompletionBlock);
    }

    // protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
    // {
    //     if (e.PropertyName == AnimationView.CacheCompositionProperty.PropertyName)
    //         PlatformView.CacheEnable = Element.CacheComposition;
    //
    //     if (e.PropertyName == AnimationView.SpeedProperty.PropertyName)
    //         PlatformView.AnimationSpeed = Element.Speed;
    //
    //     if (e.PropertyName == AnimationView.RepeatModeProperty.PropertyName)
    //         PlatformView.LoopAnimation = Element.RepeatMode == RepeatMode.Infinite;
    //
    //     if (e.PropertyName == AnimationView.ProgressProperty.PropertyName)
    //         PlatformView.AnimationProgress = Element.Progress;
    //
    //     base.OnElementPropertyChanged(sender, e);
    // }
    
    private void AnimationCompletionBlock(bool animationFinished)
    {
        if (animationFinished)
        {
            RealVirtualView?.InvokeFinishedAnimation();

            if (RealVirtualView?.RepeatMode == RepeatMode.Infinite)
            {
                RealVirtualView?.InvokeRepeatAnimation();
                RealVirtualView?.PlayWithCompletion(AnimationCompletionBlock);
            }
            else if (RealVirtualView?.RepeatMode == RepeatMode.Restart && repeatCount < RealVirtualView?.RepeatCount)
            {
                repeatCount++;
                RealVirtualView?.InvokeRepeatAnimation();
                RealVirtualView?.PlayWithCompletion(AnimationCompletionBlock);
            }
            else if (RealVirtualView?.RepeatMode == RepeatMode.Restart && repeatCount == RealVirtualView?.RepeatCount)
            {
                repeatCount = 1;
            }
        }
    }
}
