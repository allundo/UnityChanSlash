using UnityEngine;
using System.Linq;
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

public class MessageData
{
    public virtual bool isRead { get; set; }
    public virtual void Read() => isRead = true;
    protected MessageSource[] source;
    public virtual MessageSource[] Source => source;

    public MessageData(params MessageSource[] source)
    {
        isRead = false;
        this.source = source;
    }

    public static MessageData Convert(MessageSource[] source)
    {
        return new MessageData(source.Select(src => src.Convert()).ToArray());
    }

    public static MessageData Inspect(
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
        return new MessageData(new MessageSource(sentence, FaceID.NONE, fontSize, literalsPerSec, alignment, title, spriteImage, matImage, caption));
    }

    public static MessageData ItemDescription(ItemInfo itemInfo)
        => Inspect(
            itemInfo.description,
            "【" + itemInfo.name + "】 × " + itemInfo.numOfItem,
            null,
            itemInfo.uiMaterial,
            "価格\n" + itemInfo.Price + " 円",
            TextAlignmentOptions.TopLeft,
            52f
        );
}
