using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class ShieldAnimator : MobAnimator
{
    public AnimatorBool guard { get; protected set; }
    public TriggerEx shield { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        shield = new TriggerEx(anim, "Shield", 1);
        guard = new AnimatorBool(anim, "Guard");
    }
    protected override void Update()
    {
        base.Update();
        TriggerEx.SetOrderedTriggers();
    }

    public class TriggerEx : AnimatorTrigger, IComparable<TriggerEx>
    {
        protected static List<TriggerEx> triggers = new List<TriggerEx>();

        /// <summary>
        /// Fires stocked triggers having the minimum order(highest priority)
        /// </summary>
        public static void SetOrderedTriggers()
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

        public int order;

        /// <summary>
        /// Set trigger firing order to avoid the same time firing
        /// </summary>
        /// <param name="order">Sort order; lower order has higher priority</param>
        /// <returns></returns>
        public TriggerEx(Animator anim, string varName, int order = 10) : base(anim, varName)
        {
            this.order = order;
        }

        /// <summary>
        /// Just stocks and reserves this trigger
        /// </summary>
        public override void Fire()
        {
            triggers.Add(this);
        }

        public void Execute()
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
