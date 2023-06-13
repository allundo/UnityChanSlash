using UnityEngine;

public class MagicGenerator : Generator<Status>
{
    protected Param param;

    public virtual MagicStatus Fire(IStatus status)
    {
        var bullet = GetInstance(param.prefab).InitParam(param) as MagicStatus;
        bullet.SetShooter(status).OnSpawn(status.Position, status.dir);
        return bullet;
    }

    public override void DestroyAll()
    {
        pool.ForEach(t => t.GetComponent<Reactor>().Destroy());
    }
    public virtual MagicGenerator Init(GameObject bulletPool, Param param)
    {
        pool = bulletPool.transform;
        this.param = param;
        spawnPoint = Vector3.zero;
        return this;
    }
}
