public class SecretMessageData : MessageData
{
    protected MessageSource[] alterSource;
    protected SecretMessageSource secretSource;

    public int messageID { get; protected set; }

    public SecretMessageData(SecretMessageSource secretSource, int messageID) : base(secretSource.data.Convert().Source)
    {
        this.messageID = messageID;
        this.secretSource = secretSource;
    }

    public override MessageSource[] Source
    {
        get
        {
            var info = GameInfo.Instance;
            var alterID = secretSource.alterIfReadNumber;
            return info.readIDs.Contains(alterID) ? secretSource.alterData.Convert().Source : source;
        }
    }

    public override bool isRead
    {
        get => base.isRead;
        set
        {
            if (value)
            {
                var info = GameInfo.Instance;

                info.readIDs.Add(messageID);

                if (secretSource.levelUpIfRead && info.secretLevel <= secretSource.secretLevel)
                {
                    info.secretLevel = secretSource.secretLevel + 1;
                }
            }

            base.isRead = value;
        }
    }

    public bool IsValid(int floor, int secretLevel)
    {
        return secretLevel >= secretSource.secretLevel && floor == secretSource.floor;
    }
}
