public class GhostEffect : MobEffect
{
    public void OnAppear()
    {
        PlayFlash(FadeInTween());
    }

    public void OnHide()
    {
        PlayFlash(GetFadeTween(0.5f));
    }
}
