using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(MobControl))]
[RequireComponent(typeof(Animator))]
public class Commander : MonoBehaviour
{
    protected MobControl character;
    protected Animator anim;

    protected Vector3 dir = Vector3.forward;

    protected bool isCommandValid = true;
    protected bool IsIdling => currentCommand == null;


    protected Transform tf;

    protected Queue<Command> cmdQueue = new Queue<Command>();
    protected Command currentCommand = null;

    protected void Start()
    {
        character = GetComponent<MobControl>();
        anim = GetComponent<Animator>();

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
        if (Input.GetButtonDown("Jump")) return Command.jump;

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (v > 0) return Command.forward;
        if (v < 0) return Command.back;

        if (h > 0) return Command.right;
        if (h < 0) return Command.left;

        return null;
    }

    public float GetSpeed()
    {
        if (IsIdling) return 0.0f;

        return currentCommand.Speed;
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
        public static Command jump;

        public static void Init(Commander commander, float baseDuration = 0.6f)
        {
            Command.commander = commander;

            forward = new ForwardCommand(baseDuration);
            back = new BackCommand(baseDuration * 1.2f);
            right = new RightCommand(baseDuration * 1.2f);
            left = new LeftCommand(baseDuration * 1.2f);
            jump = new JumpCommand(baseDuration * 2);
        }

        public Command(float duration)
        {
            this.duration = duration;
        }

        public abstract void Execute();
        public virtual float Speed => TILE_UNIT / duration;

        protected Tween GetLinearMove(Vector3 moveVector)
        {
            return commander.tf.DOMove(moveVector, duration)
                .SetRelative()
                .SetEase(Ease.Linear);
        }

        protected Sequence GetJumpSequence(Vector3 moveVector, float jumpPower = 1.0f, float edgeTime = 0.3f, float takeoffRate = 0.01f)
        {
            float middleTime = duration - 2 * edgeTime;
            float flyingRate = 1.0f - takeoffRate;

            return DOTween.Sequence()
                .Append(
                    commander.tf.DOMove(moveVector * takeoffRate, edgeTime).SetEase(Ease.OutExpo).SetRelative()
                )
                .Append(
                    commander.tf.DOJump(moveVector * flyingRate, jumpPower, 1, middleTime).SetRelative()
                )
                .AppendInterval(edgeTime);
        }

        protected void PlayTweenMove(Tween move)
        {
            move.OnComplete(() => commander.DispatchCommand()).Play();
        }

        protected void PlaySequenceMove(Sequence move)
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

        public override float Speed => -TILE_UNIT / duration;
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

    protected class JumpCommand : Command
    {
        public JumpCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            Debug.Log("Jump");

            Vector3 jump = commander.dir * TILE_UNIT * 2;
            PlaySequenceMove(GetJumpSequence(jump));
            commander.anim.SetTrigger("Jump");

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }
}