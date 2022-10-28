public class ItemInventoryTest : ItemInventory
{
    public bool RemoveTest(int index) => inventoryItems.RemoveItem(inventoryItems.GetItem(index));
    public ItemIcon GetItem(int index) => inventoryItems.GetItem(index);
}
