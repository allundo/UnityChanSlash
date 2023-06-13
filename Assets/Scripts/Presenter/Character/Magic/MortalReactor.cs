using UnityEngine;

public interface IMortalReactor : IReactor
{
    void ReduceHP(float reduction = 1f);
    float CurrentHP { get; }
    void Die();
}

[RequireComponent(typeof(MagicStatus))]
public abstract class MortalReactor : Reactor, IMortalReactor
{
    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f) OnDie();
    }

    public float CurrentHP => status.Life.Value;

    public void ReduceHP(float reduction = 1f)
    {
        if (status.IsAlive) status.LifeChange(-reduction);
    }

    public void Die() => ReduceHP(status.LifeMax.Value);
}
