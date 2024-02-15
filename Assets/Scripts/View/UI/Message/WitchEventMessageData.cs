public class WitchEventMessageData : MessageData
{
    private readonly int knowWitch = 5;
    private readonly int wellKnowWitch = 8;

    public WitchEventMessageData(WorldMap map) : base()
    {
        bool isMsgRead = map.IsTreasureRoomMessageRead();
        int secretLevel = GameInfo.Instance.secretLevel;

        if (secretLevel < knowWitch)
        {
            source = new MessageSource[]
            {
                new MessageSource("『表の看板は読まなかったのかい？』"),
                new MessageSource((isMsgRead ? "いやまあ読んだけど・・・" : "えっ？読んでないけど・・・") + "\n誰よ？あなたは", FaceID.DEFAULT),
                new MessageSource((isMsgRead ? "" : "『えっ？読まずにどうやって・・・』\n『・・・まあいいか』\n") + "『私は迷宮の番人』\n『その鍵は返してもらう』"),
                new MessageSource("だってコレないと外に出られないじゃん？", FaceID.DESPISE),
                new MessageSource("『そっちの事情なんて知らないね！』\n『私はあなたに立ち塞がる敵』"),
                new MessageSource("『ここで逃がすわけにはいかない！』"),
                new MessageSource("ふーん・・・", FaceID.ASHAMED),
                new MessageSource("こっちは散々ひどい目にあったんだけど", FaceID.DESPISE),
                new MessageSource("あなたの目的はなに？", FaceID.DEFAULT),
                new MessageSource("『目的・・・』\n『目的・・・・？』"),
                new MessageSource("あるでしょ？目的くらい。", FaceID.ANGRY),
                new MessageSource("『知らない』\n『いいじゃん、楽しいでしょ？』"),
                new MessageSource("うーん、話にならないね。\n私だってそっちの事情なんて知らんし・・・", FaceID.EYECLOSE),
                new MessageSource("こんなとこ、とっととトンズラさせてもらうわ！", FaceID.ANGRY)
            };
        }
        else if (secretLevel < wellKnowWitch)
        {
            source = new MessageSource[]
            {
                new MessageSource("『表の看板は読まなかったの？』"),
                new MessageSource((isMsgRead ? "いやまあ読んだけど・・・" : "いや知らんけど・・・") + "\nあんたが迷宮の番人ってやつ？", FaceID.DEFAULT),
                new MessageSource("『・・・！』\n『正解、私は迷宮の番人』\n『その鍵は返してもらう』"),
                new MessageSource("いーや、こんな訳わかんないとこ、\nすぐに出てやるんだから！", FaceID.ANGRY2),
                new MessageSource("『逃さないよ』\n『私はあなたに立ち塞がる敵』"),
                new MessageSource("『あなたは永遠に・・・』\n『この迷宮をさまようといい！』"),
                new MessageSource("ふーん・・・", FaceID.ASHAMED),
                new MessageSource("すでに私以外の誰かをこの迷宮に\n閉じ込めてるみたいじゃん？", FaceID.DEFAULT),
                new MessageSource("『・・・知らない』\n『よく覚えてない』"),
                new MessageSource("覚えてないってことはないでしょ！", FaceID.ANGRY),
                new MessageSource("『知らないよ』\n『それよりお姉さん』\n『遊んでくれないの？』"),
                new MessageSource("遊びと言ったか・・・\n遊びで済んでりゃいいけどね？", FaceID.EYECLOSE),
                new MessageSource("『何を言ってるの？』\n『あなたは迷宮の挑戦者』\n『わたしは迷宮の番人・・・』"),
                new MessageSource("『・・・それだけ』\n『わたしはあなたの邪魔をする！』"),
                new MessageSource("なんかこうなるとちょっと気の毒な気もしてきた。", FaceID.NOTICE),
                new MessageSource("でもこっちのやることは変わらない。", FaceID.EYECLOSE),
                new MessageSource("こんなとこ、とっととトンズラさせてもらうわ！", FaceID.ANGRY)
            };
        }
        else
        {
            source = new MessageSource[]
            {
                new MessageSource("『いらっしゃい、大鳥こはくさん』"),
                new MessageSource("こら！ゲーム内で本名呼びはやめなさい！", FaceID.ANGRY),
                new MessageSource("『よくわからないけど・・・。』\n『私は迷宮の番人』\n『その鍵は返してもらう。』"),
                new MessageSource("・・・ウィノアちゃんだよね？\nこんなこともうやめない？", FaceID.DEFAULT),
                new MessageSource("『・・・ウィ、ノ、ア？』\n『ウィノ、ア…』\n『なんだか懐かしい名前』"),
                new MessageSource("自分の名前も忘れたの？", FaceID.NOTICE),
                new MessageSource("『ウィノア』\n『わたしは、ウィノア・・・？』"),
                new MessageSource("・・・もう一度聞くけど、\nこんな不毛な迷宮遊びやめたらどうかな？", FaceID.DESPISE),
                new MessageSource("『わたしは迷宮の番人』\n『挑戦者の魂を喰らい・・・』\n『迷宮を育てるの』"),
                new MessageSource("(なんか、意識が侵食されてるみたいだな・・・)", FaceID.EYECLOSE),
                new MessageSource("あの墓泥棒さん・・・分かるよね？\nあの人もずっと閉じ込めておくつもりなの？", FaceID.DISATTRACT),
                new MessageSource("『墓泥棒・・・アンナのこと？』\n『知らない・・・思い出してはいけない・・・』"),
                new MessageSource("アンナさんっていうのか・・・", FaceID.EYECLOSE),
                new MessageSource("いいから思い出せ！\nアンナさんだって今のあんたを哀れんでる。", FaceID.ANGRY),
                new MessageSource("『・・・ううう、知らない！』\n『・・・わたしはこの神殿から出られない』\n『アンナとずっと一緒にいる』"),
                new MessageSource("望めば一緒にいてくれたんじゃないの？\n・・・あの人なら", FaceID.DEFAULT),
                new MessageSource("(今の状態じゃ無理そうだけど)", FaceID.EYECLOSE),
                new MessageSource("『ねぇ、あなたも一緒に、』\n『いいでしょ？楽しいよ』"),
                new MessageSource("巻き込むなっ！", FaceID.ANGRY2),
                new MessageSource("『わたしは迷宮の番人・・・』\n『もう後戻りはできない・・・』"),
                new MessageSource("うーん、ダメそうだね、こりゃ。", FaceID.DISATTRACT2),
                new MessageSource("逃げよう！", FaceID.SMILE),
            };
        }
    }
}
