using UnityEngine;
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
    public Sprite spriteImage;
    public Material matImage;

    public MessageData(
        string sentence,
        FaceID face = FaceID.NONE,
        float fontSize = 64f,
        float literalsPerSec = 20f,
        TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft,
        Sprite spriteImage = null,
        Material matImage = null
    )
    {
        this.sentence = sentence;
        this.face = face;
        this.fontSize = fontSize;
        this.literalsPerSec = literalsPerSec;
        this.alignment = alignment;
        this.spriteImage = spriteImage;
        this.matImage = matImage;
    }

    public static MessageData[] Inspect(
        string sentence,
        Sprite spriteImage = null,
        Material matImage = null,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center,
        float fontsize = 72f,
        float literalsPerSec = 1000f
    )
    {
        return new MessageData[] { new MessageData(sentence, FaceID.NONE, fontsize, literalsPerSec, alignment, spriteImage, matImage) };
    }

    public static MessageData[] ItemDescription(ItemInfo itemInfo)
    {
        return Inspect(
            "【" + itemInfo.name + "】 × " + itemInfo.numOfItem + "\n\n" + itemInfo.description,
            null,
            itemInfo.material,
            TextAlignmentOptions.MidlineLeft,
            64f
        );

    }

}
