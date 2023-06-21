public class MapCompleted : ResultAnimation
{
    protected override string ValueFormat(ulong value) => Util.PercentFormat(value);
}

