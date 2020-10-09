using UnityEngine;

public class ColliderState
{
    protected CapsuleCollider col;

    protected float orgColHeight { get; }
    protected Vector3 orgColCenter { get; }

    protected float threshold;

    public ColliderState(CapsuleCollider col, float threshold = 0.001f)
    {
        this.col = col;
        this.threshold = threshold;

        orgColHeight = col.height;
        orgColCenter = col.center;
    }

    public virtual void UpdateCollider(float value = 0.0f) { }

    public void ResetCollider()
    {
        col.height = orgColHeight;
        col.center = orgColCenter;
    }
}
