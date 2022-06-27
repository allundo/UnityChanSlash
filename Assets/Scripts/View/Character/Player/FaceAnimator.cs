using UnityEngine;
using DG.Tweening;

public abstract class FaceAnimator : MobAnimator
{
    protected FaceClipsSet face;

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
    }

    protected virtual void Start()
    {
        face = ResourceLoader.Instance.faceClipsSet;
        SetParams();
    }

    protected abstract void SetParams();

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
        protected float transitionDuration;

        public FaceSwitch(FaceAnimator faceAnim, string faceName, float duration = 1f, float transitionDuration = 0.05f)
        {
            this.faceAnim = faceAnim;
            hashedFaceVar = Animator.StringToHash(faceName);
            this.duration = duration;
            this.transitionDuration = transitionDuration;
        }

        public void Fire()
        {
            faceAnim.anim.CrossFade(hashedFaceVar, transitionDuration);
            faceAnim.applyFaceLayer(duration);
        }
    }
}