public class StatusTextHidden : StatusText
{
    protected override void Awake()
    {
        base.Awake();
        valueTMP.enabled = false;
        label.enabled = false;
    }

    public override void SetTransparent()
    {
        valueTMP.enabled = label.enabled = false;
        subStatus.ForEach(status => status.SetTransparent());
    }

    public override void SetOpaque()
    {
        valueTMP.enabled = label.enabled = true;
        SetAlpha(1f);
        subStatus.ForEach(status => status.SetOpaque());
    }
}
