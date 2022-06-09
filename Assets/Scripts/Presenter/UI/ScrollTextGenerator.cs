using UnityEngine;

public class ScrollTextGenerator : MobGenerator<ScrollText>
{
    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector2.zero;
    }

    public ScrollText Spawn(Vector2 startPos, float duration, string text, Color fontColor)
    {
        return GetInstance(fixedPrefab).OnSpawn(startPos, duration, text, fontColor);
    }
}
