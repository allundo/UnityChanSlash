using UnityEngine;
using DG.Tweening;

public class LockControl : MonoBehaviour
{
    private Animator anim;
    private IMatTransparentEffect matTransparentEffect;
    private int unlock;
    private Vector3 defaultScale;
    private Tween unlockTween;

    protected void Awake()
    {
        anim = GetComponent<Animator>();
        matTransparentEffect = new MatTransparentEffect(transform);
        anim.speed = 0;
        unlock = Animator.StringToHash("Unlock");
        defaultScale = transform.localScale;
    }

    public void Reset()
    {
        matTransparentEffect.FadeIn(0.1f);
        transform.localScale = defaultScale;
    }

    public void Unlock()
    {
        anim.speed = 1f;
        anim.SetTrigger(unlock);
        unlockTween = DOTween.Sequence()
            .AppendInterval(1f)
            .AppendCallback(() => matTransparentEffect.FadeOut())
            .Append(transform.DOScale(new Vector3(defaultScale.x * 1.5f, 0f, defaultScale.z), 0.5f))
            .AppendCallback(() => anim.speed = 0f)
            .Play();
    }

    public void ForceDelete()
    {
        unlockTween?.Complete(true);
        gameObject.SetActive(false);
    }
}
