using UnityEngine;

public class FireBallGenerator : Generator<MobReactor>
{
    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector3.zero;
    }

    public BulletReactor Fire(Vector3 pos, IDirection dir, float attack = 1f)
    {
        var bullet = base.Spawn(pos, dir).SetAttack(attack);

        return bullet as BulletReactor;
    }
}
