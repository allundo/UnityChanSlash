﻿public class ResultFaceAnimator : FaceAnimator
{
    public FaceSwitch normal { get; protected set; }
    public FaceSwitch eyeClose { get; protected set; }
    public FaceSwitch smile { get; protected set; }
    public FaceSwitch surprise { get; protected set; }

    public TriggerFace drop { get; protected set; }
    public TriggerFace stagger { get; protected set; }

    protected override void SetParams()
    {
        normal = new FaceSwitch(this, face.normal.name, 10.0f, 0.2f);
        eyeClose = new FaceSwitch(this, face.eyeClose.name, 10.0f, 0.2f);
        smile = new FaceSwitch(this, face.smile1.name, 10.0f, 0.2f);
        surprise = new FaceSwitch(this, face.surprise.name, 10.0f, 0.05f);

        drop = new TriggerFace("Drop", surprise);
    }
}
