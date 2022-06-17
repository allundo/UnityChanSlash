public class TitleFaceAnimator : FaceAnimator
{
    public FaceSwitch normal { get; protected set; }
    public FaceSwitch angry { get; protected set; }
    public FaceSwitch eyeClose { get; protected set; }
    public FaceSwitch smile { get; protected set; }
    public FaceSwitch surprise { get; protected set; }

    public TriggerFace drop { get; protected set; }
    public TriggerFace stagger { get; protected set; }

    protected override void SetParams()
    {
        normal = new FaceSwitch(this, face.normal.name, 1.0f);
        angry = new FaceSwitch(this, face.angry1.name, 1.0f);
        eyeClose = new FaceSwitch(this, face.eyeClose.name, 1.0f);
        smile = new FaceSwitch(this, face.smile1.name, 1.0f);
        surprise = new FaceSwitch(this, face.surprise.name, 1.0f);

        drop = new TriggerFace("Drop", surprise);
        stagger = new TriggerFace("Stagger", surprise);
    }
}
