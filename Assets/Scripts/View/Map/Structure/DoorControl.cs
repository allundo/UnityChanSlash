using UnityEngine;
using UniRx;
using DG.Tweening;

public class DoorControl : MonoBehaviour
{
    public virtual ItemType LockType => ItemType.Null;

    protected DoorState doorState;

    protected Transform doorL;
    protected Transform doorR;

    protected Material materialGate;
    protected Material materialR;
    protected Material materialL;

    protected Renderer gateRenderer;
    protected Renderer doorRRenderer;
    protected Renderer doorLRenderer;

    protected Tween doorMove;

    public void KillTween()
    {
        doorMove?.Kill();
    }

    protected virtual void Awake()
    {
        doorR = this.transform.GetChild(0);
        doorL = this.transform.GetChild(1);

        gateRenderer = GetComponent<Renderer>();
        doorRRenderer = doorR.GetComponent<Renderer>();
        doorLRenderer = doorL.GetComponent<Renderer>();
    }

    protected virtual void Start()
    {
        doorState.State.Subscribe(state => OnStateChange(state)).AddTo(this);
        ResetAlpha();
    }

    public DoorControl SetMaterials(Material materialGate, Material materialDoor)
    {
        this.materialGate = Util.SwitchMaterial(gateRenderer, materialGate);
        this.materialR = Util.SwitchMaterial(doorRRenderer, materialDoor);
        this.materialL = Util.SwitchMaterial(doorLRenderer, materialDoor);
        return this;
    }

    public DoorControl SetState(DoorState state)
    {
        this.doorState = state;
        return this;
    }

    public void SetAlpha(float distance)
    {
        float alpha = Mathf.Clamp01(0.3f + 0.4f * distance);
        SetColorToMaterial(new Color(1, 1, 1, alpha));
    }

    public void ResetAlpha()
    {
        SetColorToMaterial(new Color(1, 1, 1, 1));
    }

    public void Handle()
    {
        if (doorState.IsControllable) doorState.TransitToNextState();
    }

    protected virtual void SetColorToMaterial(Color color)
    {
        materialGate.SetColor("_Color", color);
        materialR.SetColor("_Color", color);
        materialL.SetColor("_Color", color);
    }

    private void OnStateChange(DoorState.StateEnum state)
    {
        switch (state)
        {
            case DoorState.StateEnum.OPENING:
                doorMove = OpenTween.Play();
                break;

            case DoorState.StateEnum.CLOSING:
                doorMove = CloseTween.Play();
                break;
        }
    }

    protected Sequence OpenTween => GetDoorHandle(true);
    protected Sequence CloseTween => GetDoorHandle(false);

    protected virtual Vector3 VecL => new Vector3(0, 0, -0.75f);
    protected virtual Vector3 VecR => new Vector3(0, 0, 0.75f);

    protected virtual Sequence GetDoorHandle(bool isOpen)
    {
        Tween moveL = doorL.DOMove(isOpen ? VecL : -VecL, 0.8f)
                .SetRelative()
                .SetEase(Ease.InOutQuad);

        Tween moveR = doorR.DOMove(isOpen ? VecR : -VecR, 0.8f)
                .SetRelative()
                .SetEase(Ease.InOutQuad);

        return DOTween.Sequence()
                .Append(moveL)
                .Join(moveR)
                .OnComplete(() => doorState.TransitToNextState());
    }
}
