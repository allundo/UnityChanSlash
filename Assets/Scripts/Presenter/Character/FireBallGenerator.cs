using UnityEngine;

public class FireBallGenerator : MobGenerator<MobStatus>
{
    protected override void Awake()
    {
        base.Awake();
        spawnPoint = Vector3.zero;
    }

    public BulletStatus Fire(Vector3 pos, IDirection dir, float attack = 1f)
    {
        var bullet = base.Spawn(pos, dir) as BulletStatus;
        bullet.Attack = attack;

        return bullet;
    }
}
