using System;

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
                new MessageSource("『表の看板は読まなかったのかい？』", FaceID.NONE),
                new MessageSource((isMsgRead ? "いやまあ読んだけど・・・" : "えっ？読んでないけど・・・") + "\n誰よ？あんた", FaceID.DEFAULT),
                new MessageSource((isMsgRead ? "" : "『えっ？読まずにどうやって・・・』\n『・・・まあいいか。』\n") + "『私は迷宮の番人。』\n『その鍵は返してもらう。』", FaceID.NONE),
                new MessageSource("だってコレないと外に出られないじゃん？", FaceID.DESPISE),
                new MessageSource("『そっちの事情なんて知らないね！』\n『私はあなたに立ち塞がる敵。』", FaceID.NONE),
                new MessageSource("『ここで逃がすわけにはいかない！』", FaceID.NONE),
                new MessageSource("ふーん・・・", FaceID.ASHAMED),
                new MessageSource("こっちは散々ひどい目にあったんだけど", FaceID.DESPISE),
                new MessageSource("あんたの目的はなに？", FaceID.DEFAULT),
                new MessageSource("『目的・・・。目的・・・・？』", FaceID.NONE),
                new MessageSource("はぁ！？あるでしょ？目的くらい。", FaceID.ANGRY),
                new MessageSource("『知らない。いいじゃん、楽しいでしょう？』", FaceID.NONE),
                new MessageSource("話にならないね。\n私だってそっちの事情なんて知らんし・・・", FaceID.EYECLOSE),
                new MessageSource("こんなとこ、とっととトンズラさせてもらうわ！", FaceID.ANGRY)
            };
        }
        else if (secretLevel < wellKnowWitch)
        {
            source = new MessageSource[]
            {
                new MessageSource("『表の看板は読まなかったのかい？』", FaceID.NONE),
                new MessageSource((isMsgRead ? "いやまあ読んだけど・・・" : "いや知らんけど・・・") + "\nあんたが迷宮の番人ってやつ？", FaceID.DEFAULT),
                new MessageSource("『・・・！』\n『よくわかったね。私は迷宮の番人。』\n『その鍵は返してもらう。』", FaceID.NONE),
                new MessageSource("いーや、こんな訳わかんないとこ、\nすぐに出てやるんだから！", FaceID.ANGRY2),
                new MessageSource("『逃さないよ。』\n『私はあなたに立ち塞がる敵。』", FaceID.NONE),
                new MessageSource("『あなたは永遠に・・・』\n『この迷宮をさまようといい！』", FaceID.NONE),
                new MessageSource("ふーん・・・", FaceID.ASHAMED),
                new MessageSource("すでに私以外の誰かをこの迷宮に\n閉じ込めてるみたいじゃん？", FaceID.DEFAULT),
                new MessageSource("『・・・知らない。よく覚えてない。』", FaceID.NONE),
                new MessageSource("覚えてないってことはないでしょ！", FaceID.ANGRY),
                new MessageSource("『知らないよ。』\n『それよりお姉さんは遊んでくれないの？』", FaceID.NONE),
                new MessageSource("遊びと言ったか・・・\n遊びで済んでないんじゃない？", FaceID.EYECLOSE),
                new MessageSource("『何を言ってるの？』\n『あなたは迷宮の挑戦者』\n『わたしは迷宮の番人・・・』", FaceID.NONE),
                new MessageSource("『・・・それだけ。』\n『わたしはあなたの邪魔をする！』", FaceID.NONE),
                new MessageSource("なんかこうなるとちょっと気の毒な気もしてきた。", FaceID.NOTICE),
                new MessageSource("でもこっちのやることは変わらない。", FaceID.EYECLOSE),
                new MessageSource("こんなとこ、とっととトンズラさせてもらうわ！", FaceID.ANGRY)
            };
        }
        else
        {
            source = new MessageSource[]
            {
                new MessageSource("『いらっしゃい、大鳥こはくさん。』", FaceID.NONE),
                new MessageSource("こら！ゲーム内で本名呼びはやめなさい！", FaceID.ANGRY),
                new MessageSource("『よくわからないけど・・・。』\n『私は迷宮の番人。』\n『その鍵は返してもらう。』", FaceID.NONE),
                new MessageSource("・・・ウィノアちゃんだよね？\nこんなこともうやめない？", FaceID.DEFAULT),
                new MessageSource("『・・・、・・・ウィ、ノ、ア？』\n『ウィノ、ア…なんだか懐かしい名前。』", FaceID.NONE),
                new MessageSource("自分の名前も忘れたの？", FaceID.NOTICE),
                new MessageSource("『ウィノア』\n『わたしは、ウィノア・・・？』", FaceID.NONE),
                new MessageSource("・・・もう一度聞くけど、\nこんな不毛な迷宮遊びやめたらどうかな？", FaceID.DESPISE),
                new MessageSource("『わたしは迷宮の番人。』\n『挑戦者の魂を喰らい・・・』\n『迷宮を育てるの。』", FaceID.NONE),
                new MessageSource("(なんか、意識が侵食されてるみたいだな・・・)", FaceID.EYECLOSE),
                new MessageSource("あの墓泥棒さん・・・分かるよね？\nあの人もずっと閉じ込めておくつもりなの？", FaceID.DISATTRACT),
                new MessageSource("『墓泥棒・・・アンナのこと？』\n『知らない・・・思い出してはいけない・・・』", FaceID.NONE),
                new MessageSource("アンナさんっていうのか・・・", FaceID.EYECLOSE),
                new MessageSource("いいから思い出せ！\nアンナさんだって今のあんたを哀れんでる。", FaceID.ANGRY),
                new MessageSource("『・・・ううう、知らない！』\n『・・・わたしはこの神殿から出られない。』\n『アンナとずっと一緒にいる。』", FaceID.NONE),
                new MessageSource("望めば一緒にいてくれたんじゃないかなぁ\n・・・あの人なら", FaceID.DEFAULT),
                new MessageSource("(今の状態じゃ無理そうだけど)", FaceID.EYECLOSE),
                new MessageSource("『ねぇ、あなたも一緒に、』\n『いいでしょ？楽しいよ。』", FaceID.NONE),
                new MessageSource("巻き込むなっ！", FaceID.ANGRY2),
                new MessageSource("『わたしは迷宮の番人・・・』\n『もう後戻りはできない・・・』", FaceID.NONE),
                new MessageSource("うーん、ダメそうだね、こりゃ。", FaceID.DISATTRACT2),
                new MessageSource("逃げよう！", FaceID.SMILE),
            };
        }
    }
}
