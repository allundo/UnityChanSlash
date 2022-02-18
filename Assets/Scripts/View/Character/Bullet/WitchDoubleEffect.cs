using UnityEngine;
using DG.Tweening;

public class WitchDoubleEffect : BulletEffect
{
    protected override void StoreMaterialColors()
    {
        foreach (Renderer renderer in meshTf.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_AdditiveColor"))
                {
                    mat.color = new Color(0, 0, 0, 1);
                    flashMaterials.Add(mat);
                }
            }
        }

        defaultScale = meshTf.localScale;
    }

    public void OnAttackStart()
    {
        fireSound.PlayEx();
        emitVfx?.Play();
    }

    public void OnAttackEnd()
    {
        emitVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public override void OnActive()
    {
        PlayFlash(FadeInTween(0.01f));
    }

    public override void OnDie()
    {
        dieSound.PlayEx();
        // eraseVfx?.Play();
        PlayFlash(FadeOutTween(0.5f));
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None) { }

    public override void OnHeal(float healRatio) { }

    protected override Sequence GetFadeTween(bool isFadeIn, float duration = 0.5f)
    {
        return base.GetFadeTween(isFadeIn ? 1f : 0f, duration)
            .Join(meshTf.DOScale(isFadeIn ? defaultScale : Vector3.zero, duration));
    }
}
