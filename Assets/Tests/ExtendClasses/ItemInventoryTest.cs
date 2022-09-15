public class ItemInventoryTest : ItemInventory
{
    public bool Remove(int x, int y) => itemIndex.RemoveItem(x, y);
    public ItemIcon GetItem(int index) => itemIndex.GetItem(index);
}