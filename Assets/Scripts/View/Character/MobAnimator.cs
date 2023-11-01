using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MobAnimator : MonoBehaviour
{
    public Animator anim { get; protected set; }

    public AnimatorFloat speed { get; protected set; }
    public AnimatorBool die { get; protected set; }

    public virtual void Pause() => anim.speed = 0f;
    public virtual void Resume() => anim.speed = 1f;
    public virtual void SetSpeed(float speed) => anim.speed = speed;

    public void SetController(RuntimeAnimatorController animatorController)
        => anim.runtimeAnimatorController = animatorController;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();

        speed = new AnimatorFloat(anim, "Speed");
        die = new AnimatorBool(anim, "Die");
    }

    public class AnimatorParam
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

        public virtual void Reset()
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

    public class AnimatorTrigger : AnimatorBool
    {
        public AnimatorTrigger(Animator anim, string varName) : base(anim, varName) { }

        public override void Fire()
        {
            anim.SetTrigger(hashedVar);
        }

        public override void Reset()
        {
            anim.ResetTrigger(hashedVar);
        }

        public bool IsSet => Bool;
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
