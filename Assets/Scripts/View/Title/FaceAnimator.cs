using UnityEngine;
using DG.Tweening;

public class FaceAnimator : MobAnimator
{
    [SerializeField] private AnimationClip faceNormal = default;
    [SerializeField] private AnimationClip faceAngry = default;
    [SerializeField] private AnimationClip faceEyeClose = default;
    [SerializeField] private AnimationClip faceSmile = default;
    [SerializeField] private AnimationClip faceSuprise = default;


    public TriggerFace drop { get; protected set; }

    protected override void Awake()
    {
        anim = GetComponent<Animator>();
        drop = new TriggerFace(anim, "Drop", faceSuprise.name, 1.0f);

    }

    public class TriggerFace : AnimatorParam
    {
        protected int hashedFaceVar;
        protected int faceLayerIndex;
        protected string faceName;
        protected float duration;

        public TriggerFace(Animator anim, string varName, string faceName, float duration) : base(anim, varName)
        {
            hashedFaceVar = Animator.StringToHash(faceName);
            faceLayerIndex = anim.GetLayerIndex("Face");
            this.duration = duration;
        }

        public override void Fire()
        {
            anim.SetTrigger(hashedVar);
            anim.CrossFade(hashedFaceVar, 0.05f);
            anim.SetLayerWeight(faceLayerIndex, 1);
            DOVirtual.DelayedCall(duration, () => anim.SetLayerWeight(faceLayerIndex, 0)).Play();
        }
    }
}