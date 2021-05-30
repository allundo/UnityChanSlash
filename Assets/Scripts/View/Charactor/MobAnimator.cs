using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public abstract class MobAnimator : MonoBehaviour
{
    protected AnimeState prevState = null;
    protected AnimeState currentState = null;

    protected Animator anim;
    protected AnimeState standardState;

    private Dictionary<int, AnimeState> stateMap = new Dictionary<int, AnimeState>();

    public AnimatorFloat speed { get; protected set; }
    public AnimatorTrigger die { get; protected set; }

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        LoadAnimeState();

        speed = new AnimatorFloat(anim, "Speed");
        die = new AnimatorTrigger(anim, "Die");

    }

    protected virtual void Update()
    {
        LoadCurrentState();

        if (currentState != prevState)
        {
            prevState = currentState;
        }

        currentState.UpdateState();

        currentState = null;
    }

    private void LoadAnimeState()
    {
        var stateNameMap = GetStateNameMap();

        foreach (KeyValuePair<string, AnimeState> map in stateNameMap)
        {
            stateMap[Animator.StringToHash("Base Layer." + map.Key)] = map.Value;
        }
    }

    protected virtual Dictionary<string, AnimeState> GetStateNameMap()
    {
        standardState = new AnimeState(anim);

        return new Dictionary<string, AnimeState> {
            { "Idle", standardState },
            { "Attack", standardState },
        };
    }
    public int GetCurrentStateID()
    {
        return anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
    }

    public AnimeState LoadCurrentState()
    {
        if (currentState == null)
        {
            try
            {
                currentState = stateMap[GetCurrentStateID()];
            }
            catch (Exception e)
            {
                Debug.Log("Illegal State: " + e.Message);
            }
        }

        return currentState;
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
