using UnityEngine;
using UniRx;
using System.Linq;

public class InventoryItemsHandler : ItemIndexHandler
{
    private ItemIcon[] items;
    protected override ItemIcon[] Items => items;

    public InventoryItemsHandler(ItemInventory inventory, ItemPanel prefabItemPanel, int width, int height)
        : base(inventory, inventory.GetComponent<RectTransform>(), prefabItemPanel, width, height)
    {
        items = Enumerable.Repeat<ItemIcon>(null, MAX_ITEMS).ToArray();
    }
}
