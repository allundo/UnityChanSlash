public class StatusStoreData
{
    public float life { get; private set; }

    public StatusStoreData(float life)
    {
        this.life = life;
    }
}

[System.Serializable]
public class MobStoreData : StatusStoreData
{
    public bool isHidden { get; private set; }

    public MobStoreData(float life, bool isHidden) : base(life)
    {
        this.isHidden = isHidden;
    }
}

[System.Serializable]
public class EnemyStoreData : MobStoreData
{
    public bool isTamed { get; private set; }

    public EnemyStoreData(float life, bool isHidden, bool isTamed) : base(life, isHidden)
    {
        this.isTamed = isTamed;
    }
}