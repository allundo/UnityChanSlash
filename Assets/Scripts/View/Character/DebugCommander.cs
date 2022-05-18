using UnityEngine;

public class DebugCommander : Commander
{

    public DebugCommander(GameObject target) : base(target)
    { }

    public override void EnqueueCommand(ICommand cmd)
    {
        int count = 0;
        cmdQueue.Enqueue(cmd);
        if (cmdQueue.Count > 1)
        {
            cmdQueue.ForEach(c => Debug.Log("Command" + count++ + ": "));
        }

        if (IsIdling)
        {
            DispatchCommand();
        }
    }
}
