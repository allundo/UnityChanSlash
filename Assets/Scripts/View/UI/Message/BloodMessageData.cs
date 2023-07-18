public class BloodMessageData : MessageData
{
    public override void Read() => base.isRead = true;
    protected BloodMessageSource bloodSource;

    public BloodMessageData(BloodMessageSource bloodSource) : base(bloodSource.data.Convert().Source)
    {
        this.bloodSource = bloodSource;
    }

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

                if (bloodSource.levelUpIfRead && info.secretLevel < bloodSource.secretLevel)
                {
                    info.secretLevel = bloodSource.secretLevel;
                }
            }

            base.isRead = value;
        }
    }
}
