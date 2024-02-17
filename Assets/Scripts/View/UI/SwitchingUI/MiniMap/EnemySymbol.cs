using DG.Tweening;

public class EnemySymbol : UISymbol
{
    protected FadeTween fade;
    protected Tween prevFade = null;

    protected override void Awake()
    {
        base.Awake();
        fade = new FadeTween(gameObject, 1f);
    }

    public override void Activate()
    {
        base.Activate();
        prevFade?.Kill();
        prevFade = fade.In(0.5f, 0f).Play();
    }
    public override void Inactivate()
    {
        prevFade?.Kill();
        prevFade = fade.Out(0.5f, 0f, null, base.Inactivate).Play();
    }
}
