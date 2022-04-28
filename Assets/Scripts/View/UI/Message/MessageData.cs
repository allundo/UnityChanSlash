using TMPro;

public enum FaceID
{
    NONE = -1,
    DEFAULT = 0,
    ANGRY,
    SMILE,
    DISATTRACT,
    NOTICE
}

public struct MessageData
{
    public string sentence;
    public FaceID face;
    public float fontSize;
    public float literalsPerSec;
    public TextAlignmentOptions alignment;

    public MessageData(string sentence, FaceID face = FaceID.NONE, float fontSize = 64f, float literalsPerSec = 20f, TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)

    {
        this.sentence = sentence;
        this.face = face;
        this.fontSize = fontSize;
        this.literalsPerSec = literalsPerSec;
        this.alignment = alignment;
    }

    public static MessageData[] Board(string sentence, float fontsize = 72f, float literalsPerSec = 1000f, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        return new MessageData[] { new MessageData(sentence, FaceID.NONE, fontsize, literalsPerSec, alignment) };
    }
}
