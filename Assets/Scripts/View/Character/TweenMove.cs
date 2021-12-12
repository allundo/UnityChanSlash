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

    public Tween Move(Vector3 destPos, float timeScale = 1f, Ease ease = Ease.Linear)
    {
        return tf.DOMove(destPos, duration * timeScale).SetEase(ease);
    }

    public Tween Move(Pos destPos, float timeScale = 1f, Ease ease = Ease.Linear)
        => Move(map.WorldPos(destPos), timeScale, ease);

    /// <summary>
    /// DOTween move with updating IsObjectOn flag to destination Tile
    /// </summary>
    /// <param name="destPos">Destination Vector3 postion</param>
    /// <param name="timeScale">Normalized time scale of the move Command duration</param>
    /// <returns>Sequence can be joined or appended another behavior</returns>
    public Sequence Linear(Vector3 destPos, float timeScale = 1f)
    {
        return DOTween.Sequence()
             .AppendCallback(() => map.MoveObjectOn(destPos))
             .Join(Move(destPos, timeScale));
    }

    /// <summary>
    /// DOTween move with updating IsObjectOn flag to destination Tile
    /// </summary>
    /// <param name="destPos">Destination map postion</param>
    /// <param name="timeScale">Normalized time scale of the move Command duration</param>
    /// <returns>Sequence can be joined or appended another behavior</returns>
    public Sequence Linear(Pos destPos, float timeScale = 1f)
    {
        return DOTween.Sequence()
             .AppendCallback(() => map.MoveObjectOn(destPos))
             .Join(Move(destPos, timeScale));
    }

    public Tween TurnL => Rotate(-90);
    public Tween TurnR => Rotate(90);
    public Tween Rotate(int angle = 90)
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
    public Sequence Jump(int distance, TweenCallback DoMiddle = null, TweenCallback DoDest = null, float jumpPower = 1.0f, float edgeTime = 0.3f, float takeoffRate = 0.01f)
    {
        Vector3 moveVector = map.GetForwardVector(distance);
        float middleTime = duration - 1.5f * edgeTime;
        float flyingRate = 1.0f - takeoffRate;

        var seq = DOTween.Sequence();

        if (distance > 0)
        {
            seq.AppendCallback(() => map.MoveObjectOn(map.GetForward));
            seq.Join(DelayedCall(0.8f, DoDest));
        }

        if (distance == 2)
        {
            DoMiddle = DoMiddle ?? (() => { });

            seq.InsertCallback(0f, () => map.MoveObjectOn(map.GetForward));
            seq.Join(DelayedCall(0.4f, DoMiddle));
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

    public Sequence Drop(float startY, float endY, float stunDuration = 0.0f, float wakeUpDuration = 0.65f)
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
    public Tween FinallyCall(TweenCallback callback)
    {
        return DOVirtual.DelayedCall(duration, callback, false);
    }

    /// <summary>
    /// Reserve processing at specified timing
    /// </summary>
    /// <param name="timing">Process start timing specified by normalized (0.0f,1.0f) command duration</param>
    /// <param name="callback"></param>
    public Tween DelayedCall(float timing, TweenCallback callback)
    {
        return DOVirtual.DelayedCall(duration * timing, callback, false);
    }
}
