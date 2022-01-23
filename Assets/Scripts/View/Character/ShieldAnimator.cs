using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class ShieldAnimator : MobAnimator
{
    protected List<TriggerEx> triggers = new List<TriggerEx>();

    public AnimatorBool guard { get; protected set; }
    public AnimatorTrigger shield { get; protected set; }
    public AnimatorTrigger turnR { get; protected set; }
    public AnimatorTrigger turnL { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        shield = new TriggerEx(triggers, anim, "Shield", 1);
        guard = new AnimatorBool(anim, "Guard");
        turnR = new TriggerEx(triggers, anim, "TurnR");
        turnL = new TriggerEx(triggers, anim, "TurnL");
    }

    protected virtual void Update()
    {
        SetOrderedTriggers();
    }

    /// <summary>
    /// Fires stocked triggers having the minimum order(highest priority)
    /// </summary>
    public void SetOrderedTriggers()
    {
        if (triggers.Count == 0) return;

        triggers.Sort();

        int minOrder = triggers.First().order;

        foreach (TriggerEx trigger in triggers)
        {
            if (trigger.order > minOrder) break;

            trigger.Execute();
        }

        triggers.RemoveAll(trigger => trigger.order == minOrder);
    }

    public void ClearTriggers()
    {
        triggers.Clear();
    }

    public class TriggerEx : AnimatorTrigger, IComparable<TriggerEx>
    {
        protected List<TriggerEx> triggers;

        public int order;

        /// <summary>
        /// Set trigger firing order to avoid the same time firing
        /// </summary>
        /// <param name="order">Sort order; lower order has higher priority</param>
        /// <returns></returns>
        public TriggerEx(List<TriggerEx> triggers, Animator anim, string varName, int order = 10) : base(anim, varName)
        {
            this.triggers = triggers;
            this.order = order;
        }

        /// <summary>
        /// Just stocks and reserves this trigger
        /// </summary>
        public override void Fire()
        {
            triggers.Add(this);
        }

        public virtual void Execute()
        {
            anim.SetTrigger(hashedVar);
        }

        public int CompareTo(TriggerEx other)
        {
            if (order == other.order) return 0;
            return order > other.order ? 1 : -1;
        }
    }
}
