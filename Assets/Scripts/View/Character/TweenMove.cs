using UnityEngine;
using DG.Tweening;

public class TweenMove
{
    protected float duration;
    protected MapUtil map;
    protected Transform tf;

    public TweenMove(Transform tf, MapUtil map, float duration)
    {
        this.tf = tf;
        this.map = map;
        this.duration = duration;
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

    /// <summary>
    /// DOTween move with updating IsObjectOn flag to destination Tile
    /// </summary>
    /// <param name="destPos">Destination Vector3 postion</param>
    /// <param name="timeScale">Normalized time scale of the move Command duration</param>
    /// <returns>Sequence can be joined or appended another behavior</returns>
    public Sequence GetLinearMove(Vector3 destPos, float timeScale = 1f)
    {
        return DOTween.Sequence()
             .AppendCallback(() => map.MoveObjectOn(destPos))
             .Join(tf.DOMove(destPos, duration * timeScale).SetEase(Ease.Linear));
    }

    /// <summary>
    /// DOTween move with updating IsObjectOn flag to destination Tile
    /// </summary>
    /// <param name="destPos">Destination map postion</param>
    /// <param name="timeScale">Normalized time scale of the move Command duration</param>
    /// <returns>Sequence can be joined or appended another behavior</returns>
    public Sequence GetLinearMove(Pos destPos, float timeScale = 1f)
    {
        return DOTween.Sequence()
             .AppendCallback(() => map.MoveObjectOn(destPos))
             .Join(tf.DOMove(map.WorldPos(destPos), duration * timeScale).SetEase(Ease.Linear));
    }

    public Tween TurnL => GetRotate(-90);
    public Tween TurnR => GetRotate(90);
    public Tween GetRotate(int angle = 90)
    {
        return tf.DORotate(new Vector3(0, angle, 0), duration)
            .SetRelative()
            .SetEase(Ease.InCubic);
    }

    /// <summary>
    /// DOTween jump with updating IsObjectOn flag to destination Tile
    /// </summary>
    /// <param name="distance">Tile unit distance to jump to</param>
    /// <param name="DoMiddle">Called on entering next Tile if the jump distance is 2</param>
    /// <param name="DoDest">Called on entering destination Tile</param>
    /// <returns>Sequence can be joined or appended another behavior</returns>
    public Sequence GetJumpSequence(int distance, TweenCallback DoMiddle = null, TweenCallback DoDest = null, float jumpPower = 1.0f, float edgeTime = 0.3f, float takeoffRate = 0.01f)
    {
        Vector3 moveVector = map.GetForwardVector(distance);
        float middleTime = duration - 1.5f * edgeTime;
        float flyingRate = 1.0f - takeoffRate;

        var seq = DOTween.Sequence();

        if (distance > 0)
        {
            seq.AppendCallback(() => map.MoveObjectOn(map.GetForward));
            seq.Join(SetDelayedCall(0.8f, DoDest));
        }

        if (distance == 2)
        {
            DoMiddle = DoMiddle ?? (() => { });

            seq.InsertCallback(0f, () => map.MoveObjectOn(map.GetForward));
            seq.Join(SetDelayedCall(0.4f, DoMiddle));
        }

        var jumpSeq = DOTween.Sequence()
            .Append(
                tf.DOMove(moveVector * takeoffRate, edgeTime * 0.5f).SetEase(Ease.OutExpo).SetRelative()
            )
            .Append(
                tf.DOJump(moveVector * flyingRate, jumpPower, 1, middleTime).SetRelative()
            )
            .AppendInterval(edgeTime);

        return seq.Join(jumpSeq);
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
        return DOVirtual.DelayedCall(duration, callback, false).Play();
    }

    /// <summary>
    /// Reserve processing at specified timing
    /// </summary>
    /// <param name="timing">Process start timing specified by normalized (0.0f,1.0f) command duration</param>
    /// <param name="callback"></param>
    public Tween SetDelayedCall(float timing, TweenCallback callback)
    {
        return DOVirtual.DelayedCall(duration * timing, callback, false).Play();
    }
}
