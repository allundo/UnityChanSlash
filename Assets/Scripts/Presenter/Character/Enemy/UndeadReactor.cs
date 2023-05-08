using UnityEngine;

public class UndeadReactor
{
    private IUndeadStatus status;
    private IUndeadInput input;
    private IUndeadEffect effect;
    private IEnemyMapUtil map;
    private Collider bodyCollider;

    public UndeadReactor(IStatus status, IMobInput input, IMobEffect effect, IMobMapUtil map, Collider bodyCollider)
    {
        this.status = status as IUndeadStatus;
        this.input = input as IUndeadInput;
        this.effect = effect as IUndeadEffect;
        this.map = map as IEnemyMapUtil;
        this.bodyCollider = bodyCollider;
    }

    public bool CheckAlive(float life)
    {
        if (life > 0f) return true;

        if (status.curse > 0f)
        {
            input.InterruptSleep();
        }
        else
        {
            input.InterruptDie();
        }
        return false;
    }

    public void OnActive(EnemyStatus.ActivateOption option)
    {
        bodyCollider.enabled = !option.isSleeping;
        map.OnActive(option.isSleeping);
        effect.OnActive(option.fadeInDuration);
        input.OnActive(option);
    }

    public void OnResurrection()
    {
        status.ResetStatus();
        effect.OnResurrection();
        bodyCollider.enabled = true;
    }

    public void OnSleep(IGetExp attacker, float expObtain)
    {
        attacker?.AddExp(expObtain * 0.5f);
        effect.OnDie();
        map.ResetTile();
        bodyCollider.enabled = false;
    }
}