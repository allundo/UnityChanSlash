using UnityEngine;

public class ItemGenerator : Generator<Item>
{
    private WorldMap map;
    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector3.zero;
        map = GameManager.Instance.worldMap;
    }

    void Start()
    {
        map.deadEndPos.ForEach(kvp => Spawn(kvp.Key, kvp.Value.Backward));
        Spawn(GameManager.Instance.PlayerPos.IncY(), Direction.south);
    }

    public Item Spawn(Pos pos, IDirection dir)
    {
        var item = base.Spawn(map.WorldPos(pos), dir);
        map.GetTile(pos).PutItem(item);
        return item;
    }


    public void Turn(IDirection dir)
    {
        transform.ForEach(tf => tf.GetComponent<Item>().SetDir(dir));
    }
}
