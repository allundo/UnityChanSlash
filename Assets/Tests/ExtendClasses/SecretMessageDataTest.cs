public class SecretMessageDataTest : SecretMessageData
{
    public int AlterIfReadNumber => secretSource.alterIfReadNumber;

    public SecretMessageDataTest(SecretMessageSource secretSource, int messageID) : base(secretSource, messageID) { }
}
