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
        TriggerEx.SetPrioritizedTriggers();
    }

    public class TriggerEx : AnimatorTrigger, IComparable<TriggerEx>
    {
        protected static List<TriggerEx> triggers = new List<TriggerEx>();
        public static void SetPrioritizedTriggers()
        {
            if (triggers.Count == 0) return;

            triggers.Sort();

            int minPriority = triggers.First().priority;

            foreach (TriggerEx trigger in triggers)
            {
                if (trigger.priority > minPriority) break;

                trigger.Execute();
            }

            triggers.RemoveAll(trigger => trigger.priority == minPriority);

        }

        public int priority;
        public TriggerEx(Animator anim, string varName, int priority = 10) : base(anim, varName)
        {
            this.priority = priority;
        }

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
            if (priority == other.priority) return 0;
            return priority > other.priority ? 1 : -1;
        }
    }
}
