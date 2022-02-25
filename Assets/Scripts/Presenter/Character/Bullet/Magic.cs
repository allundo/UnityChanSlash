using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class Magic : MonoBehaviour
{
    [SerializeField] protected BulletType[] types;
    public BulletType PrimaryType => types[0];

    public Dictionary<BulletType, ILauncher> launcher { get; protected set; } = new Dictionary<BulletType, ILauncher>();

    protected virtual void Awake()
    {
        IStatus status = GetComponent<MobStatus>();

        types.ForEach(type => launcher[type] = new Launcher(status, type));
    }

    public Tween MagicSequence(BulletType type, float duration) => launcher[type].AttackSequence(duration);
    public void Fire(BulletType type) => launcher[type].Fire();
}
