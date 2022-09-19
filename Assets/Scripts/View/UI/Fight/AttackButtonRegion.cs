public class AttackButtonRegion : FadeEnable
{
    protected override void Awake()
    {
        fade = new FadeMaterialColor(gameObject, 1f);
        Inactivate();
    }
}
