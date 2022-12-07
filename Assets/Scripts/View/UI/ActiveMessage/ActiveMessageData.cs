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

public enum SDEmotionID
{
    NONE = -1,
    SURPRISE = 0,
    QUESTION,
    EXSURPRISE,
    EXQUESTION,
    SIGH,
    IRRITATE,
    WAIWAI,
    BLUE,
    CONFUSE,
}

public struct ActiveMessageData
{
    public string sentence;
    public SDFaceID face;
    public SDEmotionID emotion;
    public float fontSize;
    public float literalsPerSec;

    public ActiveMessageData(
        string sentence,
        SDFaceID face = SDFaceID.DEFAULT,
        SDEmotionID emotion = SDEmotionID.NONE,
        float fontSize = 56,
        float literalsPerSec = 40f
    )
    {
        this.sentence = sentence;
        this.face = face;
        this.emotion = emotion;
        this.fontSize = fontSize;
        this.literalsPerSec = literalsPerSec;
    }

    public static ActiveMessageData LevelUp(int level)
    {
        return new ActiveMessageData($"レベル {level + 1} に上がった！", SDFaceID.SMILE, SDEmotionID.WAIWAI);
    }

    public static ActiveMessageData GetItem(ItemInfo item)
    {
        return new ActiveMessageData(item.name + " を手に入れた！", SDFaceID.SMILE, SDEmotionID.WAIWAI);
    }

    public static ActiveMessageData TameSucceeded(IEnemyStatus status)
    {
        return new ActiveMessageData(status.Name + " を懐柔した！", SDFaceID.SMILE, SDEmotionID.WAIWAI);
    }

    public static ActiveMessageData AlreadyTamed(IEnemyStatus status)
    {
        return new ActiveMessageData(status.Name + " はすでに懐柔されている…！", SDFaceID.SURPRISE, SDEmotionID.EXQUESTION);
    }

    public static ActiveMessageData InspectTile(ITile tile)
    {
        if (tile is Pit)
        {
            var pit = tile as Pit;
            if (pit.IsOpen)
            {
                return new ActiveMessageData("落とし穴がある。", SDFaceID.DISATTRACT, SDEmotionID.BLUE);
            }
            else
            {
                return new ActiveMessageData("落とし穴はっけん！", SDFaceID.ANGRY2, SDEmotionID.SURPRISE);
            }
        }

        return new ActiveMessageData("なにもない");
    }
}
