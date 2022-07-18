public enum SDFaceID
{
    DEFAULT = 0,
    ANGRY,
    ANGRY2,
    DISATTRACT,
    EYECLOSE,
    SAD,
    SAD2,
    SMILE,
    SURPRISE
}

public struct ActiveMessageData
{
    public string sentence;
    public SDFaceID face;
    public float fontSize;
    public float literalsPerSec;

    public ActiveMessageData(
        string sentence,
        SDFaceID face = SDFaceID.DEFAULT,
        float fontSize = 56,
        float literalsPerSec = 40f
    )
    {
        this.sentence = sentence;
        this.face = face;
        this.fontSize = fontSize;
        this.literalsPerSec = literalsPerSec;
    }

    public static ActiveMessageData GetItem(ItemInfo item)
    {
        return new ActiveMessageData(item.name + " を手に入れた！", SDFaceID.SMILE);
    }
}
