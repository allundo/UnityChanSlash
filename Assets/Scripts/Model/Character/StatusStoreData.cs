public class StatusStoreData
{
    public const float LIFE_TO_BE_INIT = -1f;

    public float life { get; private set; }

    public StatusStoreData(float life = LIFE_TO_BE_INIT)
    {
        this.life = life;
    }
}

public class MobStatusStoreData : StatusStoreData
{
    public int level { get; private set; }

    public MobStatusStoreData(int level = 0, float life = LIFE_TO_BE_INIT) : base(life)
    {
        this.level = level;
    }
}

[System.Serializable]
public class EnemyStoreData : MobStatusStoreData
{
    public bool isTamed { get; private set; }
    public float curse { get; private set; }

    public EnemyStoreData(int level = 0, float life = LIFE_TO_BE_INIT, bool isTamed = false, float curse = 0) : base(level, life)
    {
        this.isTamed = isTamed;
        this.curse = curse;
    }
}
