using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;

public abstract class AttackButtonsHandler : MonoBehaviour
{
    [SerializeField] protected AttackButton[] attackButtons = default;

    public static readonly float ATTACK_CRITICAL_RATIO = Constants.PLAYER_ATTACK_SPEED / Constants.PLAYER_CRITICAL_SPEED;

    protected Target enemyTarget;
    public void SetTarget(Target enemyTarget)
    {
        this.enemyTarget = enemyTarget;
    }

    protected ICommand[] attackCmds;
    protected ICommand[] criticalCmds;

    protected void Awake()
    {
        attackCmds = new ICommand[attackButtons.Length];
        criticalCmds = new ICommand[attackButtons.Length];
    }

    protected void Start()
    {
        int length = attackButtons.Length;
        bool[,] cancelable = GetCancelableTable();

        var coolTimeTable = new Dictionary<AttackButton, Dictionary<AttackButton, float>>();

        for (int i = 0; i < length; i++)
        {
            coolTimeTable[attackButtons[i]] = new Dictionary<AttackButton, float>();

            for (int j = 0; j < length; j++)
            {
                coolTimeTable[attackButtons[i]][attackButtons[j]]
                     = cancelable[i, j] ? attackButtons[i].CancelTime : attackButtons[i].CoolTime;
            }
        }

        Observable.Merge(attackButtons.Select(button => button.ObservableAtk))
        .Subscribe(button =>
        {
            attackButtons.ForEach(
                otherBtn => otherBtn.SetCoolTime(coolTimeTable[button][otherBtn]),
                button
            );
        }).AddTo(this);
    }

    public void SetCommands(PlayerCommandTarget target)
    {
        for (int i = 0; i < attackButtons.Length; i++)
        {
            var btn = attackButtons[i];
            attackCmds[i] = new PlayerAttackCommand(target, i, btn.MotionFrames, btn.CancelStart);
            criticalCmds[i] = new PlayerCriticalAttack(target, i, btn.MotionFrames * ATTACK_CRITICAL_RATIO, btn.CancelStart);
        }
    }

    public void SetRegions(InputRegion region)
    {
        for (int i = 0; i < attackButtons.Length; i++)
        {
            attackButtons[i].SetRegion(region.AttackButtonRegion(i));
        }
    }

    public IObservable<ICommand> AttackButtons()
    {
        IObservable<ICommand> buttons = null;
        for (int i = 0; i < attackButtons.Length; i++)
        {
            ICommand normal = attackCmds[i];
            ICommand critical = criticalCmds[i];
            var button = attackButtons[i].ObservableAtk.Select(_ => enemyTarget.isPointerOn ? critical : normal);
            buttons = buttons?.Merge(button) ?? button;
        }
        return buttons;
    }

    public virtual void SetUIRadius(float radius) { }

    protected abstract bool[,] GetCancelableTable();

    public abstract AttackButton GetAttack(Vector2 uiPos);
}
