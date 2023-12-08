public class PictureMessageData : MessageData
{
    private readonly int knowWitch = 5;
    private readonly int wellKnowWitch = 8;
    protected MessageSource[] noticeSource = new MessageSource[4];
    protected MessageSource[] sadSource = new MessageSource[4];
    protected static MessageSource CreateSource(string sentence, FaceID face, bool ignoreIfRead = false)
        => new MessageSource(sentence, face, 64, 20, TMPro.TextAlignmentOptions.TopLeft, null, null, null, null, ignoreIfRead);

    public PictureMessageData() : base
    (
        CreateSource("写真のように高精彩な絵・・・の上になんだか落書きのような絵が", FaceID.DEFAULT, true),
        CreateSource("人形のような少女と手をつなぐ・・・女性？\nクレヨンみたいな画材で殴り描かれてる", FaceID.NOTICE, true),
        CreateSource("・・・", FaceID.EYECLOSE, true),
        CreateSource("なんなんだろ？この絵は？？", FaceID.DESPISE, false)
    )
    {
        for (int i = 0; i < 3; ++i)
        {
            noticeSource[i] = sadSource[i] = source[i];
        }

        noticeSource[3] = CreateSource("この子がウィノアちゃん、なんだろうね。\nで、もしかして、この人ってさっきの・・・！？", FaceID.SURPRISE);
        sadSource[3] = CreateSource("こんなに仲良さそうなのにな・・・\nどうしてこうなっちゃったんだろ", FaceID.DISATTRACT2);

    }

    // 読んだとき message level 以上だとメッセージが理解できる反応をする
    public override MessageSource[] Source
    {
        get
        {
            int level = GameInfo.Instance.secretLevel;
            if (level >= wellKnowWitch) return sadSource;
            if (level >= knowWitch) return noticeSource;
            return source;
        }
    }
}
