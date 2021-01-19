using UnityEngine;
public class MobStatus : MonoBehaviour
{
    protected MobCommander commander = default;

    public float Life { get; protected set; } = 0.0f;

    [SerializeField] public float LifeMax = 10;

    public virtual int Attack { get; protected set; } = 1;

    protected virtual void Start()
    {
        commander = GetComponent<MobCommander>();
        Life = LifeMax;
    }

    protected virtual void OnDie()
    {
        commander.SetDie();
    }

    protected virtual void OnDamage() { }

    public void Damage(int damage)
    {
        Life -= damage;

        if (Life > 0.0f)
        {
            DamageEffect(damage);
            return;
        }

        OnDie();
    }

    protected virtual void DamageEffect(int damage)
    {
    }
}