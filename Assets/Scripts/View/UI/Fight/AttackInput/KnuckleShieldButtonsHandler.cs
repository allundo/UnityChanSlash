public class KnuckleShieldButtonsHandler : KnuckleKnuckleButtonsHandler
{
    protected override bool[,] GetCancelableTable()
    {
        var cancelable = new bool[3, 3];

        // LAttack => Kick , RStraight -> LAttack are cancelable.
        cancelable[0, 2] = cancelable[1, 0] = true;

        return cancelable;
    }
}
