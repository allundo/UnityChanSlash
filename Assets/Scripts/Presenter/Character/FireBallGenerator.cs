using UnityEngine;

public class FireBallGenerator : Generator<MobStatus>
{
    protected override void Awake()
    {
        pool = transform;
        spawnPoint = Vector3.zero;
    }

    public BulletStatus Fire(Vector3 pos, IDirection dir, float attack = 1f)
    {
        var bullet = base.Spawn(pos, dir) as BulletStatus;
        bullet.Attack = attack;

        return bullet;
    }
}
