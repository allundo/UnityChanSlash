using UnityEngine;
using UniRx.Triggers;
using static UniRx.Triggers.ObservableStateMachineTrigger;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public abstract class MobAnimator : MonoBehaviour
{
    protected AnimeState prevState = null;
    protected AnimeState currentState = null;

    protected Animator anim;
    protected AnimeState standardState;
    protected ObservableStateMachineTrigger Trigger;
    protected IObservable<OnStateInfo> StateEnter => Trigger.OnStateEnterAsObservable();
    protected IObservable<OnStateInfo> StateExit => Trigger.OnStateExitAsObservable();

    private Dictionary<int, AnimeState> stateMap = new Dictionary<int, AnimeState>();

    public AnimatorFloat speed { get; protected set; }
    public AnimatorTrigger die { get; protected set; }

    public virtual void Pause() => anim.speed = 0.0f;
    protected virtual void Start() => anim.speed = 1.0f;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        Trigger = anim.GetBehaviour<ObservableStateMachineTrigger>();

        speed = new AnimatorFloat(anim, "Speed");
        die = new AnimatorTrigger(anim, "Die");
    }

    public abstract class AnimatorParam
    {
        protected int hashedVar;
        protected string varName;
        protected Animator anim;

        public AnimatorParam(Animator anim, string varName)
        {
            this.anim = anim;
            this.varName = varName;
            this.hashedVar = Animator.StringToHash(varName);
        }

        public void SetTrigger()
        {
            Fire();
        }
        public virtual void Fire()
        {
            Debug.Log(varName + " is not a Trigger");
        }

        public virtual bool Bool
        {
            get
            {
                Debug.Log(varName + " is not a Bool");
                return false;
            }
            set
            {
                Debug.Log(varName + " is not a Bool");
            }
        }
        public virtual float Float
        {
            get
            {
                Debug.Log(varName + " is not a Float");
                return 0.0f;
            }
            set
            {
                Debug.Log(varName + " is not a Float");
            }
        }
        public virtual int Int
        {
            get
            {
                Debug.Log(varName + " is not a Integer");
                return 0;
            }
            set
            {
                Debug.Log(varName + " is not a Integer");
            }
        }
    }

    public class AnimatorTrigger : AnimatorParam
    {
        public AnimatorTrigger(Animator anim, string varName) : base(anim, varName) { }

        public override void Fire()
        {
            anim.SetTrigger(hashedVar);
        }
    }

    public class AnimatorBool : AnimatorParam
    {
        public AnimatorBool(Animator anim, string varName) : base(anim, varName) { }

        public override bool Bool
        {
            get
            {
                return anim.GetBool(hashedVar);
            }
            set
            {
                anim.SetBool(hashedVar, value);
            }
        }
    }

    public class AnimatorFloat : AnimatorParam
    {
        public AnimatorFloat(Animator anim, string varName) : base(anim, varName) { }

        public override float Float
        {
            get
            {
                return anim.GetFloat(hashedVar);
            }
            set
            {
                anim.SetFloat(hashedVar, value);
            }
        }
    }

    public class AnimatorInt : AnimatorParam
    {
        public AnimatorInt(Animator anim, string varName) : base(anim, varName) { }

        public override int Int
        {
            get
            {
                return anim.GetInteger(hashedVar);
            }
            set
            {
                anim.SetInteger(hashedVar, value);
            }
        }
    }
}
