using UnityEngine;

public class BulletGenerator : Generator<MobStatus>
{
    protected MobParam param;

    public BulletStatus Fire(IStatus status)
    {
        var bullet = GetInstance(param.prefab).InitParam(param).OnSpawn(status.Position, status.dir) as BulletStatus;
        return bullet.SetShooter(status);
    }

    public override void DestroyAll()
    {
        pool.ForEach(t => t.GetComponent<Reactor>().Destroy());
    }
    public virtual BulletGenerator Init(GameObject bulletPool, MobParam param)
    {
        pool = bulletPool.transform;
        this.param = param;
        spawnPoint = Vector3.zero;
        return this;
    }
}
