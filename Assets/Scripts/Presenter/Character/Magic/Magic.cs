using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class Magic : MonoBehaviour
{
    [SerializeField] protected MagicType[] types;
    public MagicType PrimaryType => types[0];

    public Dictionary<MagicType, ILauncher> launcher { get; protected set; } = new Dictionary<MagicType, ILauncher>();

    protected virtual void Start()
    {
        IStatus status = GetComponent<MobStatus>();

        types.ForEach(type => launcher[type] = new Launcher(status, type));
    }

    public Tween FireSequence(MagicType type, float duration) => launcher[type].FireSequence(duration);
    public void Fire(MagicType type) => launcher[type].Fire();
}
