using UnityEngine;
using DG.Tweening;

public class WitchLight : SpawnObject<WitchLight>
{
    [SerializeField] private ParticleSystem lightVFX = default;
    private Tween fallDown;

    void Awake()
    {
        fallDown = DOTween.Sequence()
            .AppendCallback(() => lightVFX.PlayEx())
            .Append(transform.DOMoveY(-0.7f, 0.75f).SetRelative().SetEase(Ease.Linear))
            .InsertCallback(0.7f, () => lightVFX.StopEmitting())
            .AppendCallback(Inactivate)
            .AsReusable(gameObject);
    }

    public override WitchLight OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0f)
    {
        transform.position = pos;
        Activate();
        fallDown.Restart();
        return this;
    }
}