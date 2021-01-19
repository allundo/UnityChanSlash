using UnityEngine;

public class MobControl : MonoBehaviour
{
    protected MobCommander commander = default;

    protected virtual void Start()
    {
        commander = GetComponent<MobCommander>();
    }
    protected virtual void Update()
    {
        commander.InputCommand();
        commander.SetSpeed();
    }
}
