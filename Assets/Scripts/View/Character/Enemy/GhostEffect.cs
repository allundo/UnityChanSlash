using UnityEngine;

public class GhostEffect : MobEffect
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected AudioSource attackSnd = default;

    protected Vector3 prevPos;
    protected float trailStrength = 0f;
    protected float trailTarget = 0f;
    protected int propID;


    protected override void Awake()
    {
        StoreMaterialColors();
        propID = Shader.PropertyToID("_TrailDir");
    }

    protected virtual void FixedUpdate()
    {
        trailStrength = 0.04f * trailTarget + 0.96f * trailStrength;
        var dir = (prevPos - transform.position).normalized * trailStrength;
        flashMaterials.ForEach(mat => mat.SetVector(propID, dir));
        prevPos = transform.position;
    }

    public void OnAppear()
    {
        PlayFlash(FadeInTween());
    }

    public void OnHide()
    {
        PlayFlash(GetFadeTween(0.5f));
    }

    public void OnAttackStart()
    {
        emitVfx?.Play();
        attackSnd.PlayEx();
        trailTarget = 1.8f;
    }

    public void OnAttackEnd()
    {
        emitVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        trailTarget = 0f;
    }

    public override void OnActive()
    {
        PlayFlash(FadeInTween());
        trailTarget = 0f;
        flashMaterials.ForEach(mat => mat.SetVector(propID, Vector4.zero));
        prevPos = transform.position;
    }
}
