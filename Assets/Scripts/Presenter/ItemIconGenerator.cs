using UnityEngine;
using System.Linq;

public class ItemIconGenerator : Generator<UISymbol>
{
    private WorldMap map;
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

    protected override void Awake()
    {
        pool = transform;
        rectTransform = GetComponent<RectTransform>();

        var defaultSize = rectTransform.sizeDelta;
        unit = new Vector2(defaultSize.x / WIDTH, defaultSize.y / HEIGHT);
        origin = new Vector2(unit.x, -unit.y) * 0.5f;

        spawnPoint = Vector3.zero;
        map = GameManager.Instance.worldMap;
    }

    public bool PickUp(Item item)
    {
        for (int j = 0; j < HEIGHT; j++)
        {
            for (int i = 0; i < WIDTH; i++)
            {
                int index = i + WIDTH * j;
                if (items[index] == null)
                {
                    items[index] = Spawn(i, j).SetItemInfo(item.itemInfo).SetMaterial(item.material);
                    return true;
                }
            }
        }

        return false;
    }

    public ItemIcon Spawn(int x, int y)
    {
        if (!IsValidPos(x, y)) return null;

        return base.Spawn(UIPos(x, y)) as ItemIcon;
    }


    private bool IsValidPos(int x, int y)
        => x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;
    private bool IsValidPos(Pos pos) => IsValidPos(pos.x, pos.y);
}
