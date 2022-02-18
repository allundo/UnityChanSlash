using UnityEngine;

public class BulletGenerator : Generator<MobStatus>
{
    protected MobParam param;

    public BulletStatus Fire(Vector3 pos, IDirection dir, float attack = 1f)
    {
        var bullet = base.Spawn(param.prefab, pos, dir) as BulletStatus;
        bullet.Attack = attack;

        return bullet;
    }

    public override void DestroyAll()
    {
        pool.ForEach(t => t.GetComponent<MobReactor>().Destroy());
    }
    public virtual BulletGenerator Init(GameObject bulletPool, MobParam param)
    {
        pool = bulletPool.transform;
        this.param = param;
        spawnPoint = Vector3.zero;
        return this;
    }
}
