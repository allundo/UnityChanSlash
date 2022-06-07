public class GameClearTime : ResultAnimation
{
    protected override string ValueFormat(ulong sec)
    {
        int min = (int)(sec / 60);
        int hour = min / 60;
        return $"{hour,3:D}:{min % 60:00}:{sec % 60:00}";
    }
}