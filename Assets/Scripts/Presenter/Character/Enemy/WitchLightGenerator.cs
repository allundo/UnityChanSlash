using UnityEngine;

public class WitchLightGenerator : MobGenerator<WitchLight>
{
    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector3.zero;
    }
    public override WitchLight Spawn(Vector3 offset, IDirection dir = null, float duration = 0f)
        => Spawn(fixedPrefab, offset); // Generate light from neck position

    public override void DestroyAll()
    {
        pool.ForEach(t => t.gameObject.GetComponent<WitchLight>().Inactivate());
    }
}