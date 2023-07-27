public class BloodMessageData : MessageData
{
    public override void Read() => base.isRead = true;
    protected BloodMessageSource bloodSource;

    public BloodMessageData(BloodMessageSource bloodSource) : base(bloodSource.data.Convert().Source)
    {
        this.bloodSource = bloodSource;
    }

    // 読んだとき message level 以上だとメッセージが理解できる反応をする
    public override MessageSource[] Source
        => bloodSource.secretLevel > GameInfo.Instance.secretLevel ? bloodSource.alterData.Convert().Source : source;

    public override bool isRead
    {
        get => base.isRead;
        set
        {
            if (value)
            {
                var info = GameInfo.Instance;

                // 読んだとき message level 未満だが、理解できる内容だった場合に secret level を message level まで引き上げる
                if (bloodSource.levelUpIfRead && info.secretLevel < bloodSource.secretLevel)
                {
                    info.secretLevel = bloodSource.secretLevel;
                }
            }

            base.isRead = value;
        }
    }
}
