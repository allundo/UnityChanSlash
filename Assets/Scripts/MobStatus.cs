using UnityEngine;
public abstract class MobStatus : MonoBehaviour
{

    protected enum StateEnum
    {
        Normal,
        Attack,
        Damage,
        Die
    }

    protected StateEnum state = StateEnum.Normal;
    protected Animator anim;
    public float Life { get; protected set; } = 0.0f;

    [SerializeField] public float LifeMax { get; } = 10;

    public bool IsMovable => StateEnum.Normal == state;
    public bool IsAttackable => StateEnum.Normal == state;

    protected virtual void Start()
    {
        Life = LifeMax;
        anim = GetComponentInChildren<Animator>();

    }

    protected virtual void OnDie()
    {
    }
    protected virtual void OnDamage() { }

    public void Damage(int damage)
    {
        if (state == StateEnum.Die) return;

        Life -= damage;

        if (Life > 0.0f)
        {
            GoToKnockBackStateIfPossible();
            return;
        }

        state = StateEnum.Die;
        anim.SetTrigger("Die");

        OnDie();
    }

    public void GoToKnockBackStateIfPossible()
    {
        state = StateEnum.Damage;
        anim.SetTrigger("Damage");
        OnDamage();
    }

    public void GoToAttackStateIfPossible()
    {
        if (!IsAttackable) return;

        state = StateEnum.Attack;
        anim.SetTrigger("Attack");
    }

    public void GoToNormalStateIfPossible()
    {
        if (state == StateEnum.Die) return;

        state = StateEnum.Normal;
    }
}