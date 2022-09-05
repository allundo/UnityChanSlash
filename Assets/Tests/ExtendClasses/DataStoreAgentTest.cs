public class DataStoreAgentTest : DataStoreAgent
{
    public void SaveEncryptedRecordTest<T>(T record, string fileName) => SaveEncryptedRecord(record, fileName);
}
