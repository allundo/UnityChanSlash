using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public abstract class AnimeHandler : MonoBehaviour
{
    protected Animator anim;
    protected CapsuleCollider col;
    protected AnimeState currentState;
    protected AnimeState standardState;

    private Dictionary<int, AnimeState> stateMap = new Dictionary<int, AnimeState>();

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();

        LoadAnimeState();
    }

    private void Update()
    {
        SetCurrentState();
        currentState.UpdateState();
    }

    private void LoadAnimeState()
    {
        var stateNameMap = GetStateNameMap();

        foreach (KeyValuePair<string, AnimeState> map in stateNameMap)
        {
            stateMap[Animator.StringToHash("Base Layer." + map.Key)] = map.Value;
        }

        currentState = stateNameMap["Idle"];
    }

    protected virtual Dictionary<string, AnimeState> GetStateNameMap()
    {
        standardState = new AnimeState(anim, new ColliderState(col));

        return new Dictionary<string, AnimeState> {
            { "Idle", standardState },
            { "Attack", standardState },
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

            if (currentState != nextState)
            {
                currentState = nextState;
                currentState.ResetCollider();
            }
        }
        catch (Exception e)
        {
            Debug.Log("Illegal State: " + e.Message);
        }
    }
}