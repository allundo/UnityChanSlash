using UnityEngine;

public class LightLaserEffect : MagicEffectFX
{
    [SerializeField] protected ParticleSystem laserVfx = default;
    [SerializeField] protected ParticleSystem wallVfx = default;
    [SerializeField] protected AudioSource laserSnd = default;

    [SerializeField] protected Transform tfLaser = default;
    [SerializeField] protected Transform tfLaserLead = default;
    protected Transform tfWall;
    protected Vector3 laserScale;
    protected Vector3 laserLeadScale;

    void Awake()
    {
        tfWall = wallVfx.transform;
        laserScale = tfLaser.localScale;
        laserLeadScale = tfLaserLead.localScale;
    }

    public void SetLength(int length)
    {
        tfLaserLead.localPosition = new Vector3(0, 0, -0.5f * (length + 1));
        tfLaser.localScale = new Vector3(laserScale.x, laserScale.y, 0.2f * (length + 1));
        tfLaserLead.localScale = new Vector3(laserLeadScale.x, laserLeadScale.y, 0.2f * (length + 1));

        if (length < LightLaserStatus.MAX_LENGTH)
        {
            tfWall.localPosition = new Vector3(0, 1f, length * Constants.TILE_UNIT);
            tfWall.gameObject.SetActive(true);
        }
        else
        {
            tfWall.gameObject.SetActive(false);
        }
    }

    protected override void OnDisappear()
    {
        wallVfx.StopEmitting();
        laserVfx.StopEmitting();
        laserSnd.StopEx();
    }

    public override void OnActive()
    {
        wallVfx.PlayEx();
        laserVfx.PlayEx();
        laserSnd.PlayEx();
    }
}
