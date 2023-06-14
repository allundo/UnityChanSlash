using UnityEngine;
using DG.Tweening;

public class DoorControl : HandleStructure
{
    protected Transform doorL;
    protected Transform doorR;

    protected Material materialGate;
    protected Material materialR;
    protected Material materialL;

    protected Renderer gateRenderer;
    protected Renderer doorRRenderer;
    protected Renderer doorLRenderer;

    protected Color defaultGateColor;
    protected Color defaultDoorColor;

    protected Collider doorCollider;

    protected virtual void Awake()
    {
        doorR = this.transform.GetChild(0);
        doorL = this.transform.GetChild(1);

        gateRenderer = GetComponent<Renderer>();
        doorRRenderer = doorR.GetComponent<Renderer>();
        doorLRenderer = doorL.GetComponent<Renderer>();

        doorCollider = GetComponent<Collider>();
    }

    protected override void Start()
    {
        base.Start();
        ResetAlpha();
    }

    public DoorControl SetMaterials(Material materialGate, Material materialDoor)
    {
        this.materialGate = Util.SwitchMaterial(gateRenderer, materialGate);
        this.materialR = Util.SwitchMaterial(doorRRenderer, materialDoor);
        this.materialL = Util.SwitchMaterial(doorLRenderer, materialDoor);

        this.defaultGateColor = this.materialGate.color;
        this.defaultDoorColor = this.materialR.color;

        return this;
    }

    public void SetAlpha(float distance)
    {
        float alpha = Mathf.Clamp01(0.3f + 0.4f * distance);
        SetAlphaToMaterial(alpha);
    }

    public void ResetAlpha()
    {
        SetAlphaToMaterial(1f);
    }

    protected virtual void SetAlphaToMaterial(float alpha)
    {
        materialGate.SetColor("_Color", new Color(defaultGateColor.r, defaultGateColor.g, defaultGateColor.b, alpha));
        var doorColor = new Color(defaultDoorColor.r, defaultDoorColor.g, defaultDoorColor.b, alpha);
        materialR.SetColor("_Color", doorColor);
        materialL.SetColor("_Color", doorColor);
    }

    protected virtual Vector3 VecL => new Vector3(0, 0, -0.75f);
    protected virtual Vector3 VecR => new Vector3(0, 0, 0.75f);

    protected override Tween GetDoorHandle(bool isOpen)
    {
        Tween moveL = doorL.DOMove(isOpen ? VecL : -VecL, 0.8f)
                .SetRelative()
                .SetEase(Ease.InOutQuad);

        Tween moveR = doorR.DOMove(isOpen ? VecR : -VecR, 0.8f)
                .SetRelative()
                .SetEase(Ease.InOutQuad);

        return DOTween.Sequence()
                .AppendCallback(() => GameManager.Instance.PlaySnd(isOpen ? SNDType.DoorOpen : SNDType.DoorClose, transform.position))
                .Append(moveL)
                .Join(moveR)
                .AppendCallback(() => handleState.TransitToNextState());
    }

    protected override void ForceOpen()
    {
        doorL.position += VecL;
        doorR.position += VecR;
        handleState.TransitToNextState();
    }

    protected override void ForceBreak()
    {
        doorL.gameObject.SetActive(false);
        doorR.gameObject.SetActive(false);
        doorCollider.enabled = false;
        handleState.TransitToNextState();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        var laser = other.GetComponent<LightLaserAttack>();
        if (laser != null && !handleState.IsOpen)
        {
            ParticleSystem vfx = ResourceLoader.Instance.LoadVFX(VFXType.DoorDestruction, transform);
            vfx.transform.rotation = laser.dir.Rotate;
            vfx.PlayEx();

            (handleState as DoorState).Break();

            GameManager.Instance.RedrawPlates();
        }
    }
}
