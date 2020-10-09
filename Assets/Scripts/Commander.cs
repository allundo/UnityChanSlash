using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(MobControl))]
public class Commander : MonoBehaviour
{
    protected MobControl character;
    protected Vector3 dir = Vector3.forward;

    protected bool isCommandValid = true;
    protected bool IsIdling => currentCommand == null;


    protected Transform tf;

    protected Queue<Command> cmdQueue = new Queue<Command>();
    protected Command currentCommand = null;

    protected void Start()
    {
        character = GetComponent<MobControl>();

        tf = character.transform;

        Debug.Log("Position: " + tf.position);

        Command.Init(this);
    }

    public void InputCommand()
    {
        if (!isCommandValid)
        {
            return;
        }

        Command cmd = GetCommand();

        Debug.Log("Cmd: " + cmd);

        if (cmd != null)
        {
            cmdQueue.Enqueue(cmd);
            isCommandValid = false;

            if (IsIdling)
            {
                DispatchCommand();
            }
        }
    }

    public bool DispatchCommand()
    {
        if (cmdQueue.Count > 0)
        {
            currentCommand = cmdQueue.Dequeue();
            currentCommand.Execute();
            return true;
        }

        currentCommand = null;
        return false;
    }

    private Command GetCommand()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (v > 0) return Command.forward;
        if (v < 0) return Command.back;

        if (h > 0) return Command.right;
        if (h < 0) return Command.left;

        return null;
    }

    protected abstract class Command
    {
        protected const float TILE_UNIT = 2.5f;

        protected float duration;

        public static Commander commander;

        public static Command forward;
        public static Command back;
        public static Command right;
        public static Command left;

        public static void Init(Commander commander, float baseDuration = 0.8f)
        {
            Command.commander = commander;

            forward = new ForwardCommand(baseDuration);
            back = new BackCommand(baseDuration);
            right = new RightCommand(baseDuration);
            left = new LeftCommand(baseDuration);
        }

        public Command(float duration)
        {
            this.duration = duration;
        }

        public abstract void Execute();

        protected Tween GetLinearMove(Vector3 moveVector)
        {
            return commander.tf.DOMove(moveVector, duration)
                .SetRelative()
                .SetEase(Ease.Linear);
        }

        protected void PlayTweenMove(Tween move)
        {
            move.OnComplete(() => commander.DispatchCommand()).Play();
        }
    }

    protected class ForwardCommand : Command
    {
        public ForwardCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            Debug.Log("Forward");

            Vector3 forward = commander.dir * TILE_UNIT;
            PlayTweenMove(GetLinearMove(forward));

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }

    protected class BackCommand : Command
    {
        public BackCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            Debug.Log("Backward");

            Vector3 back = -commander.dir * TILE_UNIT;
            PlayTweenMove(GetLinearMove(back));

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }

    protected class RightCommand : Command
    {
        public RightCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            Debug.Log("Right");

            Vector3 right = Quaternion.Euler(0, 90, 0) * commander.dir * TILE_UNIT;
            PlayTweenMove(GetLinearMove(right));

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }

    protected class LeftCommand : Command
    {
        public LeftCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            Debug.Log("Left");

            Vector3 left = Quaternion.Euler(0, -90, 0) * commander.dir * TILE_UNIT;
            PlayTweenMove(GetLinearMove(left));

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }
}