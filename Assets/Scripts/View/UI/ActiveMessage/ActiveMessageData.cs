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

    public static ActiveMessageData InspectTile(ITile tile)
    {
        if (tile is Pit)
        {
            var pit = tile as Pit;
            if (pit.IsOpen)
            {
                return new ActiveMessageData("落とし穴がある。", SDFaceID.DISATTRACT);
            }
            else
            {
                return new ActiveMessageData("落とし穴はっけん！", SDFaceID.ANGRY2);
            }
        }

        return new ActiveMessageData("なにもない");
    }
}
