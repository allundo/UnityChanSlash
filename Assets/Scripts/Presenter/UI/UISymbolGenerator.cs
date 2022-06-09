using UnityEngine;

public class UISymbolGenerator : MobGenerator<UISymbol>
{
    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector2.zero;
    }

    public UISymbolGenerator SetParent(Transform parentTransform)
    {
        pool = parentTransform;
        return this;
    }

    public UISymbol Spawn(Vector2 offsetPos, float duration = 0.5f) => base.Spawn(offsetPos, null, duration);
}
