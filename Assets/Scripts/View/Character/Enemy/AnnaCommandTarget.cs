using DG.Tweening;

public class AnnaCommandTarget : CommandTarget
{
    /// <summary>
    /// The tween sets forward and backward move speed.
    /// </summary>
    public Tween speedTween { get; protected set; }

    /// <summary>
    /// The tween sets left and right move speed.
    /// </summary>
    public Tween speedTweenLR { get; protected set; }

    public void SetSpeed(Tween speedTween)
    {
        if (this.speedTween != speedTween) this.speedTween?.Kill();
        this.speedTween = speedTween?.Play();
    }

    public void SetSpeedLR(Tween speedTweenLR)
    {
        if (this.speedTweenLR != speedTweenLR) this.speedTweenLR?.Kill();
        this.speedTweenLR = speedTweenLR?.Play();
    }
}
