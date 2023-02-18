using UnityEngine;
using DG.Tweening;

public class HologramFade : MaterialEffect
{
    public static bool isHologramOn = false;

    public HologramFade(Transform targetTf) : base(targetTf)
    {
        InitEffects();
    }

    protected override string propName => "_HologramColor";
    protected override void InitProperty(Material mat, int propID) => mat.SetColor(propID, new Color(0f, 1f, 0.4f, 0f));

    protected Tween fadeIn;
    protected Tween fadeOut;

    public void SetActive(bool isActive)
    {
        if (isActive && isHologramOn)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }

    private void FadeIn()
    {
        fadeOut?.Kill();
        fadeIn = DOFade(true);
    }
    private void FadeOut()
    {
        fadeIn?.Kill();
        fadeOut = DOFade(false);
    }

    private Tween DOFade(bool isIn)
    {
        if (materials.Count == 0) return null;

        var current = materials[0].GetColor(propID);

        return DOVirtual.Color(
            current,
            new Color(current.r, current.g, current.b, isIn ? 1f : 0f),
            0.2f * (isIn ? 1f - current.a : current.a),
            color => materials.ForEach(mat => mat.SetColor(propID, color))
        ).Play();
    }

    public override void KillAllTweens()
    {
        fadeIn?.Kill();
        fadeOut?.Kill();
    }
}
