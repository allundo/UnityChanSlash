using UnityEngine;
using DG.Tweening;

public class DoorDestructFX : SpawnObject<DoorDestructFX>
{
    [SerializeField] private ParticleSystem destructVfx = default;
    [SerializeField] private ParticleSystemRenderer chunkRenderer = default;
    [SerializeField] private AudioSource destructSnd = default;

    private Tween fxTimer;

    public override DoorDestructFX OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        transform.position = pos;
        transform.rotation = dir != null ? dir.Rotate : Quaternion.identity;

        Activate();

        destructVfx.PlayEx();
        destructSnd.PlayEx();

        fxTimer = DOVirtual.DelayedCall(duration, () =>
        {
            destructVfx.StopAndClear();
            destructSnd.StopEx();
            Inactivate();
        }).Play();

        return this;
    }

    public void SetMaterial(Material mat)
    {
        Util.SwitchMaterial(chunkRenderer, mat);
    }

    public void CompleteTween()
    {
        fxTimer?.Complete();
    }
}
