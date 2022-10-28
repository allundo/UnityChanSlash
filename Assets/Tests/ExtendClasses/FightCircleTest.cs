using UnityEngine;

public class FightCircleTest : FightCircle
{
    // Apply Awake() after injecting modules.
    protected override void Awake() { }

    public FightCircleTest InjectModules(AttackInputController attackInputUI, RectTransform forwardUIRT)
    {
        this.attackInputUI = attackInputUI;
        this.forwardUIRT = forwardUIRT;

        // attackInputUI is used in Awake();
        base.Awake();

        return this;
    }
}
