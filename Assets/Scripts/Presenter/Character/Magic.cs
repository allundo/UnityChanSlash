using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class Magic : MonoBehaviour
{
    [SerializeField] protected BulletType[] types;
    public Dictionary<BulletType, IAttack> launcher { get; protected set; } = new Dictionary<BulletType, IAttack>();

    protected virtual void Awake()
    {
        IStatus status = GetComponent<MobStatus>();
        types.ForEach(type => launcher[type] = new Launcher(status, type));
    }

    public Tween MagicSequence(BulletType type, float duration) => launcher[type].AttackSequence(duration);
}
