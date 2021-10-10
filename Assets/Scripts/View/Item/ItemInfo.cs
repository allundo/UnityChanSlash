public class ItemInfo
{
    public ItemType type { get; private set; }
    public int numOfItem { get; private set; }

    public ItemInfo(ItemType type, int numOfItem = 1)
    {
        this.type = type;
        this.numOfItem = numOfItem;
    }

    public int UseItem()
    {
        return --numOfItem;
    }
}