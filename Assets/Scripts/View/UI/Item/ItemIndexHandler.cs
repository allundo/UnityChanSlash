using UnityEngine;
using System.Linq;

public class ItemIndexHandler
{
    public int WIDTH { get; private set; }
    public int HEIGHT { get; private set; }
    public int MAX_ITEMS { get; private set; }
    public Pos INVALID_INDEX { get; private set; }

    private ItemIcon[] items;

    private Vector2 unit;
    private Vector2 origin;
    private Vector2 inventoryOrigin;

    public ItemIndexHandler(RectTransform rt, int width, int height)
    {
        this.WIDTH = width;
        this.HEIGHT = height;
        MAX_ITEMS = width * height;

        INVALID_INDEX = new Pos(WIDTH, HEIGHT);

        items = Enumerable.Repeat<ItemIcon>(null, MAX_ITEMS).ToArray();

        var defaultSize = rt.sizeDelta;
        unit = new Vector2(defaultSize.x / WIDTH, defaultSize.y / HEIGHT);

        // Anchor of ItemIcon is set to left top by default on prefab
        origin = new Vector2(unit.x, -unit.y) * 0.5f;

        inventoryOrigin = new Vector2(rt.position.x - defaultSize.x * 0.5f, rt.position.y + defaultSize.y * 0.5f);
    }

    public Vector2 ConvertToVec(Vector2 screenPos) => screenPos - inventoryOrigin;

    public Vector2 UIPos(int index) => UIPos(Index(index));
    public Vector2 UIPos(int x, int y) => origin + new Vector2(unit.x * x, -unit.y * y);
    public Vector2 UIPos(Pos pos) => UIPos(pos.x, pos.y);

    public bool IsValidIndex(int x, int y)
        => x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;
    public bool IsValidIndex(Pos pos) => IsValidIndex(pos.x, pos.y);

    public int Index(Pos index) => index.x + WIDTH * index.y;
    public Pos Index(int index) => new Pos(index % WIDTH, index / WIDTH);

    public void SetItem(int index, ItemIcon itemIcon)
    {
        items[index] = itemIcon;
    }

    public ItemIcon GetItem(Pos index) => GetItem(index.x, index.y);
    public ItemIcon GetItem(int x, int y) => IsValidIndex(x, y) ? items[x + WIDTH * y] : null;
    public ItemIcon GetItem(int index) => index < MAX_ITEMS ? items[index] : null;

    public bool RemoveItem(Pos index) => RemoveItem(index.x, index.y);
    public bool RemoveItem(int x, int y)
    {
        var itemIcon = GetItem(x, y);
        if (itemIcon == null) return false;

        itemIcon.Inactivate();
        items[x + WIDTH * y] = null;
        return true;
    }

    public bool UseItem(Pos index) => UseItem(index.x, index.y);
    public bool UseItem(int x, int y) => UseItem(x + WIDTH * y);
    public bool UseItem(int index)
    {
        var itemIcon = GetItem(index);

        if (itemIcon == null) return false;

        if (itemIcon.UseItem()) items[index] = null;

        return true;
    }
}