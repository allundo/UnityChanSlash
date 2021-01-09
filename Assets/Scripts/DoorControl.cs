using UnityEngine;
using DG.Tweening;

public class DoorControl : MonoBehaviour
{
    protected enum StateEnum
    {
        OPEN,
        CLOSE,
        OPENING,
        CLOSING,
    }

    protected StateEnum state;
    protected bool isLocked;

    public bool IsOpen { get; protected set; } = false;

    protected bool IsControllable;
    protected Transform doorL;
    protected Transform doorR;

    protected virtual Vector3 VecL => new Vector3(0, 0, -0.75f);
    protected virtual Vector3 VecR => new Vector3(0, 0, 0.75f);

    void Start()
    {
        isLocked = false;
        IsOpen = false;
        state = StateEnum.CLOSE;

        doorR = this.transform.GetChild(0);
        doorL = this.transform.GetChild(1);
    }

    void Update()
    {
        if (IsOpen && state == StateEnum.CLOSE) Open();
        if (!IsOpen && state == StateEnum.OPEN) Close();
    }

    public void Handle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }

    }

    public void Open()
    {
        if (state != StateEnum.CLOSE) return;

        Tween moveL = doorL.DOMove(VecL, 0.8f)
                .SetRelative()
                .SetEase(Ease.InOutQuad);

        Tween moveR = doorR.DOMove(VecR, 0.8f)
                .SetRelative()
                .SetEase(Ease.InOutQuad);

        Sequence open = DOTween.Sequence()
                .Append(moveL)
                .Join(moveR)
                .OnComplete(() => { state = StateEnum.OPEN; });

        state = StateEnum.OPENING;
        IsOpen = true;
        open.Play();
    }

    public void Close()
    {
        if (state != StateEnum.OPEN) return;

        Tween moveL = doorL.DOMove(-VecL, 0.8f)
                .SetRelative()
                .SetEase(Ease.InOutQuad);

        Tween moveR = doorR.DOMove(-VecR, 0.8f)
                .SetRelative()
                .SetEase(Ease.InOutQuad);

        Sequence open = DOTween.Sequence()
                .Append(moveL)
                .Join(moveR)
                .OnComplete(() => { state = StateEnum.CLOSE; });

        state = StateEnum.CLOSING;
        IsOpen = false;
        open.Play();
    }
}

