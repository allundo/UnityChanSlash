public class AttackButtonRegion : FadeUI
{
    protected override void Awake()
    {
        FadeInit(new FadeMaterialColor(gameObject, maxAlpha));
    }
}
