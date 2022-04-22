using UnityEngine;

public class MatVertexEffect : MaterialEffect
{
    protected override string propName => "_TrailDir";
    protected override void InitProperty(Material mat, int propID) => mat.SetVector(propID, Vector4.zero);
    public MatVertexEffect(Transform targetTf) : base(targetTf)
    {
        transform = targetTf;
    }

    protected Transform transform;
    protected Vector3 prevPos;
    protected float trailStrength;
    public float trailTarget;

    public void Update()
    {
        trailStrength = 0.04f * trailTarget + 0.96f * trailStrength;
        var dir = (prevPos - transform.position).normalized * trailStrength;
        materials.ForEach(mat => mat.SetVector(propID, dir));
        prevPos = transform.position;
    }

    public override void InitEffects()
    {
        base.InitEffects();
        prevPos = transform.position;
        trailStrength = trailTarget = 0f;
    }

    public override void KillAllTweens() { }
}