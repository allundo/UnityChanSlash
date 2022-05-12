using UnityEngine;
using DG.Tweening;

public class FaceAnimator : MobAnimator
{
    [SerializeField] private AnimationClip faceNormal = default;
    [SerializeField] private AnimationClip faceAngry = default;
    [SerializeField] private AnimationClip faceEyeClose = default;
    [SerializeField] private AnimationClip faceSmile = default;
    [SerializeField] private AnimationClip faceSurprise = default;

    public FaceSwitch normal { get; protected set; }
    public FaceSwitch angry { get; protected set; }
    public FaceSwitch eyeClose { get; protected set; }
    public FaceSwitch smile { get; protected set; }
    public FaceSwitch surprise { get; protected set; }

    public TriggerFace drop { get; protected set; }
    public TriggerFace stagger { get; protected set; }

    protected Tween faceLayerCanceler;
    protected int faceLayerIndex;

    public Tween applyFaceLayer(float duration = 1f)
    {
        faceLayerCanceler?.Kill();
        anim.SetLayerWeight(faceLayerIndex, 1);
        faceLayerCanceler = DOVirtual.DelayedCall(duration, () => anim.SetLayerWeight(faceLayerIndex, 0)).Play();

        return faceLayerCanceler;
    }

    protected override void Awake()
    {
        anim = GetComponent<Animator>();
        faceLayerIndex = anim.GetLayerIndex("Face");

        normal = new FaceSwitch(this, faceNormal.name, 1.0f);
        angry = new FaceSwitch(this, faceAngry.name, 1.0f);
        eyeClose = new FaceSwitch(this, faceEyeClose.name, 1.0f);
        smile = new FaceSwitch(this, faceSmile.name, 1.0f);
        surprise = new FaceSwitch(this, faceSurprise.name, 1.0f);

        drop = new TriggerFace("Drop", surprise);
        stagger = new TriggerFace("Stagger", surprise);
    }

    public class TriggerFace : AnimatorParam
    {
        FaceSwitch faceSwitch;
        public TriggerFace(string varName, FaceSwitch faceSwitch) : base(faceSwitch.faceAnim.anim, varName)
        {
            this.faceSwitch = faceSwitch;
        }

        public override void Fire()
        {
            anim.SetTrigger(hashedVar);
            faceSwitch.Fire();
        }
    }

    public class FaceSwitch
    {
        public FaceAnimator faceAnim { get; protected set; }
        protected int hashedFaceVar;
        protected float duration;

        public FaceSwitch(FaceAnimator faceAnim, string faceName, float duration)
        {

            this.faceAnim = faceAnim;
            hashedFaceVar = Animator.StringToHash(faceName);
            this.duration = duration;
        }

        public void Fire()
        {
            faceAnim.anim.CrossFade(hashedFaceVar, 0.05f);
            faceAnim.applyFaceLayer(duration);
        }
    }
}