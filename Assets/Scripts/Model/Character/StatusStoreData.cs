public class StatusStoreData
{
    public float life { get; private set; }

    public StatusStoreData(float life)
    {
        this.life = life;
    }
}

public class MobStatusStoreData : StatusStoreData
{
    public int level { get; private set; }

    public MobStatusStoreData(float life, int level) : base(life)
    {
        this.level = level;
    }
}

[System.Serializable]
public class EnemyStoreData : MobStatusStoreData
{
    public bool isTamed { get; private set; }

    public EnemyStoreData(float life, int level, bool isTamed) : base(life, level)
    {
        this.isTamed = isTamed;
    }
}