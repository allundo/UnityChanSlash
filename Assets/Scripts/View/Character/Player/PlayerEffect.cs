using UnityEngine;

public class PlayerEffect : MobEffect
{
    [SerializeField] private Transform tfListener = default;

    protected override void Awake()
    {
        matColEffect = new MobMatColorEffect(transform);
        sndData = Resources.Load<PlayerDamageSndData>("DataAssets/Sound/PlayerDamageSndData");
        animFX = GetComponent<AnimationFX>();
        resourceFX = new ResourceFX(transform);
        anim = GetComponent<PlayerAnimator>();
    }

    protected override AudioSource SndInstance(AudioSource src) => Util.Instantiate(src, tfListener);

    protected override AudioSource SndDamage(AttackType type, IDirection dir = null)
    {
        var src = damageSndSource.LazyLoad(type, SndDamageInstance);
        var offset = dir != null ? dir.LookAt : Vector3.zero;
        src.transform.position = tfListener.position - offset;
        return src;
    }
    protected override AudioSource SndCritical(AttackType type, IDirection dir = null)
    {
        var src = criticalSndSource.LazyLoad(type, SndCriticalInstance);
        var offset = dir != null ? dir.LookAt : Vector3.zero;
        src.transform.position = tfListener.position - offset;
        return src;
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None, IDirection dir = null)
    {
        base.OnDamage(damageRatio, type, attr, dir);
        (anim as PlayerAnimator).rest.Bool = false;
    }

    public override void OnActive()
    {
        matColEffect.Activate(0f);
    }

    public void OnIced(Vector3 pos, bool isPaused)
    {
        if (isPaused) anim.Pause();
        resourceFX.PlayVFX(VFXType.Iced, pos);
        matColEffect.Flash(new Color(0f, 0.5f, 0.5f, 1f), 0.1f);
    }

    public void OnShield() => (animFX as ShieldAnimFX).OnShield();

    public void OnDrop()
    {
        resourceFX.PlaySnd(SNDType.DropToGround);
    }

    public void OnGround()
    {
        resourceFX.StopSnd(SNDType.DropToGround);
    }
}
