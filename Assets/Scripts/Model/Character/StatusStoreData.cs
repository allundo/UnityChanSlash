public class StatusStoreData
{
    public float life { get; private set; }

    public StatusStoreData(float life = 0)
    {
        this.life = life;
    }
}

public class MobStatusStoreData : StatusStoreData
{
    public int level { get; private set; }

    public MobStatusStoreData(int level = 0, float life = 0) : base(life)
    {
        this.level = level;
    }
}

[System.Serializable]
public class EnemyStoreData : MobStatusStoreData
{
    public bool isTamed { get; private set; }
    public float curse { get; private set; }

    public EnemyStoreData(int level = 0, float life = 0, bool isTamed = false, float curse = 0) : base(level, life)
    {
        this.isTamed = isTamed;
        this.curse = curse;
    }
}
