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
}
