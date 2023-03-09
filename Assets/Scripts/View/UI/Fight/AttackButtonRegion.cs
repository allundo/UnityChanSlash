public class AttackButtonRegion : FadeUI
{
    protected override FadeTween FadeComponent() => new FadeMaterialColor(gameObject, uiAlpha * maxAlpha);
}
