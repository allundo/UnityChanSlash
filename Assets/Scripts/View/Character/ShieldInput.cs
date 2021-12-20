using UnityEngine;

[RequireComponent(typeof(ShieldAnimator))]
public abstract class ShieldInput : MobInput
{
    public GuardState guardState { get; protected set; }

    public bool IsShield => commander.currentCommand is ShieldCommand;

    public override bool IsFightValid => IsIdling || IsShield;

    protected virtual void Start()
    {
        SetInputs();
    }

    /// <summary>
    /// This method is called by Start(). Override it for UniRx subscription. <br />
    /// Mainly used for input definition.
    /// </summary>
    protected virtual void SetInputs()
    {
        guardState = new GuardState(this);
    }

    public class GuardState
    {
        protected ShieldInput input;
        protected MapUtil map;
        private Command shieldOn;

        public virtual bool IsShieldOn(IDirection attackDir) => input.IsShield && map.dir.IsInverse(attackDir);

        public GuardState(ShieldInput input, float duration = 15f)
        {
            this.input = input;
            map = input.target.map;
            shieldOn = new ShieldOnCommand(input.target, duration);
        }

        public void SetShield() => input.Interrupt(shieldOn);
    }
}