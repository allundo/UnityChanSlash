using UnityEngine;
using DG.Tweening;

public class TweenMove
{
    protected float duration;

    protected Transform tf;

    public TweenMove(Transform tf, float duration)
    {
        this.duration = duration;
        this.tf = tf;
    }


    public Sequence JoinTweens(params Tween[] tweens)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < tweens.Length; i++)
        {
            seq.Join(tweens[i]);
        }

        return seq;
    }

    public Tween GetLinearMove(Vector3 moveVector)
    {
        return tf.DOMove(moveVector, duration)
            .SetRelative()
            .SetEase(Ease.Linear);
    }

    public Tween GetRotate(int angle = 90)
    {
        return tf.DORotate(new Vector3(0, angle, 0), duration)
            .SetRelative()
            .SetEase(Ease.InCubic);
    }

    public Sequence GetJumpSequence(Vector3 moveVector, float jumpPower = 1.0f, float edgeTime = 0.3f, float takeoffRate = 0.01f)
    {
        float middleTime = duration - 1.5f * edgeTime;
        float flyingRate = 1.0f - takeoffRate;

        return DOTween.Sequence()
            .Append(
                tf.DOMove(moveVector * takeoffRate, edgeTime * 0.5f).SetEase(Ease.OutExpo).SetRelative()
            )
            .Append(
                tf.DOJump(moveVector * flyingRate, jumpPower, 1, middleTime).SetRelative()
            )
            .AppendInterval(edgeTime);
    }
    public Sequence GetDropMove(float startY, float endY, float stunDuration = 0.0f, float wakeUpDuration = 0.65f)
    {
        float fallDuration = duration - stunDuration - wakeUpDuration;

        return DOTween.Sequence()
            .Append(tf.DOMoveY(startY, 0f).SetRelative())
            .Append(tf.DOMoveY(endY - startY, fallDuration).SetRelative().SetEase(Ease.InQuad))
            .AppendInterval(stunDuration + wakeUpDuration);
    }

    /// <summary>
    /// Reserve processing after command duration
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public Tween SetFinallyCall(TweenCallback callback)
    {
        return DOVirtual.DelayedCall(duration, callback).Play();
    }

    /// <summary>
    /// Reserve processing at specified timing
    /// </summary>
    /// <param name="timing">Process start timing specified by normalized (0.0f,1.0f) command duration</param>
    /// <param name="callback"></param>
    public Tween SetDelayedCall(float timing, TweenCallback callback)
    {
        return DOVirtual.DelayedCall(duration * timing, callback).Play();
    }
}