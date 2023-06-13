using UnityEngine;

public class BulletGenerator : Generator<Status>
{
    protected Param param;

    public virtual BulletStatus Fire(IStatus status)
    {
        var bullet = GetInstance(param.prefab).InitParam(param) as BulletStatus;
        bullet.SetShooter(status).OnSpawn(status.Position, status.dir);
        return bullet;
    }

    public override void DestroyAll()
    {
        pool.ForEach(t => t.GetComponent<Reactor>().Destroy());
    }
    public virtual BulletGenerator Init(GameObject bulletPool, Param param)
    {
        pool = bulletPool.transform;
        this.param = param;
        spawnPoint = Vector3.zero;
        return this;
    }
}
