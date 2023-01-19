using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachineTrigger : StateMachineBehaviour
{
    protected Dictionary<int, List<Action<AnimatorStateInfo>>> onStateEnter = new Dictionary<int, List<Action<AnimatorStateInfo>>>();
    protected Dictionary<int, List<Action<AnimatorStateInfo>>> onStateExit = new Dictionary<int, List<Action<AnimatorStateInfo>>>();

    public void AddStateEnter(int fullPathHash, Action<AnimatorStateInfo> action)
        => AddStateTrigger(onStateEnter, fullPathHash, action);

    public void RemoveStateEnter(int fullPathHash, Action<AnimatorStateInfo> action)
        => RemoveStateTrigger(onStateEnter, fullPathHash, action);

    public void ClearStateEnter(int fullPathHash) => ClearStateTrigger(onStateEnter, fullPathHash);

    public void AddStateExit(int fullPathHash, Action<AnimatorStateInfo> action)
        => AddStateTrigger(onStateExit, fullPathHash, action);

    public void RemoveStateExit(int fullPathHash, Action<AnimatorStateInfo> action)
        => RemoveStateTrigger(onStateExit, fullPathHash, action);

    public void ClearStateExit(int fullPathHash) => ClearStateTrigger(onStateExit, fullPathHash);

    protected void AddStateTrigger(Dictionary<int, List<Action<AnimatorStateInfo>>> onStateTrigger, int fullPathHash, Action<AnimatorStateInfo> action)
    {
        List<Action<AnimatorStateInfo>> actions = null;

        if (!onStateTrigger.TryGetValue(fullPathHash, out actions))
        {
            onStateTrigger[fullPathHash] = new List<Action<AnimatorStateInfo>>();
        }
        onStateTrigger[fullPathHash].Add(action);
    }

    protected void RemoveStateTrigger(Dictionary<int, List<Action<AnimatorStateInfo>>> onStateTrigger, int fullPathHash, Action<AnimatorStateInfo> action)
    {
        List<Action<AnimatorStateInfo>> actions = null;

        if (!onStateEnter.TryGetValue(fullPathHash, out actions)) return;

        if (actions.Count <= 1)
        {
            ClearStateTrigger(onStateTrigger, fullPathHash);
            return;
        }

        onStateTrigger[fullPathHash].Remove(action);
    }

    protected void ClearStateTrigger(Dictionary<int, List<Action<AnimatorStateInfo>>> onStateTrigger, int fullPathHash)
        => onStateTrigger.Remove(fullPathHash);

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => OnStateTrigger(onStateEnter, stateInfo);

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => OnStateTrigger(onStateExit, stateInfo);

    protected void OnStateTrigger(Dictionary<int, List<Action<AnimatorStateInfo>>> onStateTrigger, AnimatorStateInfo stateInfo)
    {
        List<Action<AnimatorStateInfo>> actions = null;
        if (onStateTrigger.TryGetValue(stateInfo.fullPathHash, out actions))
        {
            actions.ForEach(action => action(stateInfo));
        }
    }
}
