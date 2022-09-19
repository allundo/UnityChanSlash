using UnityEngine;

public class FightCircleTest : FightCircle
{
    public FightCircleTest InjectModules(AttackInputController attackInputUI, RectTransform forwardUIRT)
    {
        this.attackInputUI = attackInputUI;
        this.forwardUIRT = forwardUIRT;
        return this;
    }
}
