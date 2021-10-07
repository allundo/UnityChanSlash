using UnityEngine;
using System.Linq;

[RequireComponent(typeof(ItemIconGenerator))]
public class ItemInventory : MonoBehaviour
{
    private ItemIconGenerator iconGenerator;
    private RectTransform rectTransform;

    private static readonly int WIDTH = 5;
    private static readonly int HEIGHT = 6;

    private ItemIcon[] items = Enumerable.Repeat<ItemIcon>(null, WIDTH * HEIGHT).ToArray();

    private Vector2 unit;
    private Vector2 origin;

    private Vector2 UIPos(int x, int y) => origin + new Vector2(unit.x * x, -unit.y * y);
    private Vector2 UIPos(Pos pos) => UIPos(pos.x, pos.y);

    public ItemIcon GetItem(Pos pos)
        => IsValidPos(pos) ? items[pos.x + WIDTH * pos.y] : null;

    private bool IsValidPos(int x, int y)
        => x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;
    private bool IsValidPos(Pos pos) => IsValidPos(pos.x, pos.y);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        iconGenerator = GetComponent<ItemIconGenerator>();
        iconGenerator.Init(transform);

        var defaultSize = rectTransform.sizeDelta;
        unit = new Vector2(defaultSize.x / WIDTH, defaultSize.y / HEIGHT);

        // Anchor of ItemIcon is set to left top by default on prefab
        origin = new Vector2(unit.x, -unit.y) * 0.5f;
    }

    public bool PickUp(Item item)
    {
        for (int j = 0; j < HEIGHT; j++)
        {
            for (int i = 0; i < WIDTH; i++)
            {
                if (SetItem(i, j, item)) return true;
            }
        }

        return false;
    }

    private bool SetItem(int x, int y, Item item)
    {
        if (!IsValidPos(x, y)) return false;

        int index = x + WIDTH * y;

        if (items[index] != null) return false;

        items[index] = iconGenerator.Spawn(UIPos(x, y), item);
        return true;
    }
}
