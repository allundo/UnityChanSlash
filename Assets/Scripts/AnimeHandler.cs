using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Commander))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class AnimeHandler : MonoBehaviour
{
    protected Animator anim;
    protected CapsuleCollider col;
    protected AnimeState state;

    protected AnimeState standardState;

    protected struct AnimeStateTypes
    {
        public AnimeState idleState;
        public AnimeState forwardState;
        public AnimeState backState;
        public AnimeState jumpState;
        public AnimeState attackState;
        public AnimeState restState;
    }

    protected Commander commander;

    private Dictionary<int, AnimeState> stateMap = new Dictionary<int, AnimeState>();

    private void Start()
    {
        anim = GetComponent<Animator>();
        commander = GetComponent<Commander>();
        col = GetComponent<CapsuleCollider>();

        LoadAnimeState();
    }

    private void Update()
    {
        anim.SetFloat("Speed", commander.GetSpeed());
        state.UpdateState();
    }

    private void LoadAnimeState()
    {
        standardState = new AnimeState(anim, new ColliderState(col));

        AnimeStateTypes types = GetAnimeStateTypes();

        Dictionary<string, AnimeState> bufMap = new Dictionary<string, AnimeState> {
            { "Idle", types.idleState },
            { "Locomotion", types.forwardState },
            { "WalkBack", types.backState },
            { "Jump", types.jumpState },
            { "Attack", types.attackState },
            { "Rest", types.restState }
        };

        foreach (KeyValuePair<string, AnimeState> map in bufMap)
        {
            stateMap[Animator.StringToHash("Base Layer." + map.Key)] = map.Value;
        }

        state = standardState;
    }

    protected virtual AnimeStateTypes GetAnimeStateTypes()
    {
        return new AnimeStateTypes
        {
            idleState = standardState,
            forwardState = standardState,
            backState = standardState,
            jumpState = standardState,
            attackState = standardState,
            restState = standardState
        };
    }

    public int GetCurrentStateID()
    {
        return anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
    }

    private void SetCurrentState()
    {
        try
        {
            AnimeState nextState = stateMap[GetCurrentStateID()];

            if (state != nextState)
            {
                state = nextState;
                state.ResetCollider();
            }
        }
        catch (Exception e)
        {
            Debug.Log("Illegal State: " + e.Message);
        }
    }
}