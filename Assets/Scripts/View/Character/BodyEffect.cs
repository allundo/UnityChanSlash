using DG.Tweening;

public interface IEffect
{
    void OnActive();
    void OnDestroyByReactor();
    void Disappear(TweenCallback onComplete = null, float duration = 0.5f);
}

public interface IBodyEffect : IEffect
{
    /// <summary>
    /// Play body effect on damage
    /// </summary>
    /// <param name="damageRatio">Normalized damage ratio to the life max</param>
    void OnDamage(float damageRatio, AttackType type, AttackAttr attr);
    void OnDie();
}
