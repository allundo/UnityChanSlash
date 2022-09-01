using UnityEngine;
using TMPro;

public enum FaceID
{
    NONE = -1,
    DEFAULT = 0,
    ANGRY,
    SMILE,
    DISATTRACT,
    NOTICE,
    EYECLOSE,
    ANGRY2,
    DISATTRACT2,
    DESPISE,
    ASHAMED,
    SURPRISE
}

public struct MessageData
{
    public string sentence;
    public FaceID face;
    public float fontSize;
    public float literalsPerSec;
    public TextAlignmentOptions alignment;
    public string title;
    public Sprite spriteImage;
    public Material matImage;
    public string caption;
    public bool ignoreIfRead;
    public bool isRead;

    public MessageData(
        string sentence,
        FaceID face = FaceID.NONE,
        float fontSize = 64f,
        float literalsPerSec = 20f,
        TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft,
        string title = null,
        Sprite spriteImage = null,
        Material matImage = null,
        string caption = null,
        bool ignoreIfRead = false
    )
    {
        this.sentence = sentence;
        this.face = face;
        this.fontSize = fontSize;
        this.literalsPerSec = literalsPerSec;
        this.alignment = alignment;
        this.title = title;
        this.spriteImage = spriteImage;
        this.matImage = matImage;
        this.caption = caption;
        this.ignoreIfRead = ignoreIfRead;
        this.isRead = false;
    }

    public static MessageData[] Inspect(
        string sentence,
        string title,
        Sprite spriteImage = null,
        Material matImage = null,
        string caption = null,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center,
        float fontSize = 72f,
        float literalsPerSec = 1000f
    )
    {
        return new MessageData[] { new MessageData(sentence, FaceID.NONE, fontSize, literalsPerSec, alignment, title, spriteImage, matImage, caption) };
    }

    public static MessageData[] ItemDescription(ItemInfo itemInfo)
    {
        return Inspect(
            itemInfo.description,
            "【" + itemInfo.name + "】 × " + itemInfo.numOfItem,
            null,
            itemInfo.material,
            "価格\n" + itemInfo.Price + " 円",
            TextAlignmentOptions.TopLeft,
            52f
        );
    }
}
