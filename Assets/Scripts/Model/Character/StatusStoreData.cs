public class StatusStoreData
{
    public float life { get; private set; }

    public StatusStoreData(float life)
    {
        this.life = life;
    }
}

[System.Serializable]
public class EnemyStoreData : StatusStoreData
{
    public bool isTamed { get; private set; }

    public EnemyStoreData(float life, bool isTamed) : base(life)
    {
        this.isTamed = isTamed;
    }
}