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
}
