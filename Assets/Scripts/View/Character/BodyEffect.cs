using DG.Tweening;

public interface IBodyEffect
{
    void OnActive();
    void OnDie();

    /// <summary>
    /// Play body effect on damage
    /// </summary>
    /// <param name="damageRatio">Normalized damage ratio to the life max</param>
    void OnDamage(float damageRatio, AttackType type, AttackAttr attr);

    void OnDestroy();
    void Disappear(TweenCallback onComplete = null, float duration = 0.5f);
}