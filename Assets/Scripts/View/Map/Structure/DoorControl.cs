using UnityEngine;
using UniRx;
using DG.Tweening;

public class DoorControl : MonoBehaviour
{
    private DoorState doorState;

    public bool IsOpen { get; protected set; } = false;

    protected Transform doorL;
    protected Transform doorR;

    protected Material materialGate;
    protected Material materialR;
    protected Material materialL;

    void Awake()
    {
        doorState = GetComponent<DoorState>();
        doorState.State.Subscribe(state => OnStateChange(state)).AddTo(this);

        doorR = this.transform.GetChild(0);
        doorL = this.transform.GetChild(1);

        materialGate = GetComponent<Renderer>().material;
        materialR = doorR.GetComponent<Renderer>().material;
        materialL = doorL.GetComponent<Renderer>().material;
    }

    void Start()
    {
        ResetAlpha();
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

    protected void SetColorToMaterial(Color color)
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
                OpenTween.Play();
                break;

            case DoorState.StateEnum.CLOSING:
                CloseTween.Play();
                break;
        }
    }

    private Sequence OpenTween => GetDoorHandle(true);
    private Sequence CloseTween => GetDoorHandle(false);

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
                .OnComplete(() => doorState.TransitionNext());
    }
}
