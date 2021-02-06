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
    protected float alpha;
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

        ResetAlpha();
    }

    void Update()
    {
        if (IsOpen && state == StateEnum.CLOSE) Open();
        if (!IsOpen && state == StateEnum.OPEN) Close();
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
        GetComponent<Renderer>().material.SetColor("_Color", color);
        doorR.GetComponent<Renderer>().material.SetColor("_Color", color);
        doorL.GetComponent<Renderer>().material.SetColor("_Color", color);
    }



    public void Handle()
    {
        if (IsOpen)
        {
            if(!GameManager.Instance.worldMap.GetTile(this.transform.position).IsCharactorOn)
            {
                Close();
            }
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

