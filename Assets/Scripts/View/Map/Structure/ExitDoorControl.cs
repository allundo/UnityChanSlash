using UnityEngine;
using UniRx;

public class ExitDoorControl : DoorControl
{
    private LockControl lockControl = default;
    private Material materialExit;
    private Color defaultExitColor;

    private Vector3 vecL;

    public ExitDoorControl SetDir(IDirection dir)
    {
        vecL = dir.Left.LookAt * 0.75f;
        return this;
    }

    protected override void Awake()
    {
        base.Awake();

        materialExit = this.transform.GetChild(2).GetComponent<Renderer>().material;
        lockControl = this.transform.GetChild(3).GetComponent<LockControl>();
        defaultExitColor = materialExit.color;
        SetDir(Direction.north);
    }
    protected override void Start()
    {
        base.Start();
        handleState.LockedState.Subscribe(isLocked => Lock(isLocked)).AddTo(this);
    }

    protected override void SetAlphaToMaterial(float alpha)
    {
        base.SetAlphaToMaterial(alpha);
        materialExit.SetColor("_Color", new Color(defaultExitColor.r, defaultExitColor.g, defaultExitColor.b, alpha));
    }

    protected override Vector3 VecL => vecL;
    protected override Vector3 VecR => -vecL;

    protected void Lock(bool isLocked)
    {
        if (isLocked)
        {
            lockControl.Reset();
        }
        else
        {
            lockControl.Unlock();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerReactor>() != null)
        {
            GameManager.Instance.Exit();
        }
    }
}
