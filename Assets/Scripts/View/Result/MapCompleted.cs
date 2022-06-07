public class MapCompleted : ResultAnimation
{
    protected override string ValueFormat(ulong value) => $"{value / 10,3:D}.{value % 10}";
}

