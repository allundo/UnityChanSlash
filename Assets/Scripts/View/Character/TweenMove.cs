using UnityEngine;
using DG.Tweening;

public class TweenMove
{
    public static readonly float TILE_UNIT = Constants.TILE_UNIT;

    protected float duration;
    protected IMapUtil map;
    protected Transform tf;

    public TweenMove(Transform tf, IMapUtil map, float duration)
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
    /// Nat a tilewise moving.
    /// </summary>
    /// <param name="distance">Move distance to forward</param>
    /// <param name="timeScale">Normalized time scale of the move Command duration</param>
    /// <returns></returns>
    public Tween MoveForward(float distance, float timeScale = 1f, TweenCallback onComplete = null)
        => Move(tf.position + tf.forward * distance, timeScale).OnComplete(onComplete);

    /// <summary>
    /// DOTween move with updating IsObjectOn flag to destination Tile
    /// </summary>
    /// <param name="destPos">Destination Vector3 postion</param>
    /// <param name="timeScale">Normalized time scale of the move Command duration</param>
    /// <returns>Playing tween for handling</returns>
    public Tween Linear(Vector3 destPos, float timeScale = 1f, TweenCallback onComplete = null)
    {
        map.MoveObjectOn(destPos);
        return Move(destPos, timeScale).OnComplete(onComplete).Play();
    }

    /// <summary>
    /// DOTween move with updating IsObjectOn flag to destination Tile
    /// </summary>
    /// <param name="destPos">Destination map position</param>
    /// <param name="timeScale">Normalized time scale of the move Command duration</param>
    /// <returns>Playing tween for handling</returns>
    public Tween Linear(Pos destPos, float timeScale = 1f, TweenCallback onComplete = null)
    {
        map.MoveObjectOn(destPos);
        return Move(destPos, timeScale).OnComplete(onComplete).Play();
    }

    /// <summary>
    /// DOTween brake with updating IsObjectOn flag to destination Tile
    /// </summary>
    /// <returns>Playing tween for handling</returns>
    public Tween Brake(Pos destPos, float timeScale = 1f, TweenCallback onComplete = null)
    {
        var toDestVec3 = map.WorldPos(destPos) - map.CurrentVec3Pos;

        map.MoveObjectOn(destPos);
        return DOTween.Sequence()
            // Set distances of (Linear : OutQuart) = (2 : 1) with (1 : 1) durations for smooth velocity connecting
            .Append(Move(toDestVec3 * 0.666667f, timeScale * 0.5f).SetRelative())
            .Append(Move(toDestVec3 * 0.333333f, timeScale * 0.5f, Ease.OutQuad).SetRelative())
            .AppendCallback(onComplete)
            .Play();
    }

    public Tween BrakeAndBack(float timeScale = 1f, TweenCallback onComplete = null)
    {
        var moveVec = map.dir.LookAt * TILE_UNIT * 0.2f;

        return DOTween.Sequence()
            .Append(Move(moveVec, timeScale * 0.5f, Ease.OutQuad).SetRelative())
            .Append(Move(-moveVec, timeScale * 0.5f, Ease.OutQuad).SetRelative())
            .AppendCallback(onComplete)
            .Play();
    }

    public Tween TurnLB => Rotate(-180f);
    public Tween TurnRB => Rotate(180f);

    public Tween Rotate(float angle = 90, float timeScale = 1f)
    {
        return tf.DORotate(new Vector3(0, angle, 0), duration * timeScale).SetRelative();
    }

    public Tween TurnToDir => tf.DORotate(map.dir.Angle, duration);

    /// <summary>
    /// DOTween jump with updating IsObjectOn flag to destination Tile
    /// </summary>
    /// <param name="distance">Tile unit distance to jump to</param>
    /// <param name="DoMiddle">Called on entering next Tile if the jump distance is 2</param>
    /// <param name="DoDest">Called on entering destination Tile</param>
    /// <returns>Playing Tween for handling</returns>
    public Tween Jump(int distance, TweenCallback DoMiddle = null, TweenCallback DoDest = null, float jumpPower = 1.0f, float edgeTime = 0.3f, float takeoffRate = 0.01f)
    {
        Vector3 moveVector = map.GetForwardVector(distance);
        float middleTime = duration - 1.5f * edgeTime;
        float flyingRate = 1.0f - takeoffRate;

        var seq = DOTween.Sequence();

        if (distance == 1)
        {
            map.MoveObjectOn(map.GetForward);
            seq.Join(DelayedCall(0.8f, DoDest));
        }

        if (distance == 2)
        {
            map.MoveObjectOn(map.GetJump);
            seq.Join(DelayedCall(0.4f, DoMiddle));
            seq.Join(DelayedCall(0.8f, DoDest));
        }

        var jumpSeq = DOTween.Sequence()
            .Append(
                tf.DOMove(moveVector * takeoffRate, edgeTime * 0.5f).SetEase(Ease.OutExpo).SetRelative()
            )
            .Append(
                tf.DOJump(moveVector * flyingRate, jumpPower, 1, middleTime).SetRelative()
            )
            .AppendInterval(edgeTime);

        return seq.Join(jumpSeq).SetUpdate(false).Play();
    }

    public Tween JumpRelative(Vector3 moveVec, float timeScale = 1f, float jumpPower = 1.0f)
        => tf.DOJump(moveVec, jumpPower, 1, duration * timeScale).SetRelative();

    public Tween JumpRelative(Pos moveVec, float timeScale = 1f, float jumpPower = 1.0f)
        => JumpRelative(map.WorldPos(moveVec), timeScale, jumpPower);

    public Tween Jump(Vector3 destPos, float timeScale = 1f, float jumpPower = 1.0f)
        => tf.DOJump(destPos, jumpPower, 1, duration * timeScale);

    public Tween Jump(Pos destPos, float timeScale = 1f, float jumpPower = 1.0f)
        => tf.DOJump(map.WorldPos(destPos), jumpPower, 1, duration * timeScale);

    public Sequence Drop(float startY, float endY, float stunDuration = 0.0f, float wakeUpDuration = 0.65f)
    {
        float fallDuration = duration - stunDuration - wakeUpDuration;

        return DOTween.Sequence()
            .Append(tf.DOMoveY(startY, 0f).SetRelative())
            .Append(tf.DOMoveY(endY - startY, fallDuration).SetRelative().SetEase(Ease.InQuad))
            .AppendInterval(stunDuration + wakeUpDuration);
    }

    public Tween Teleport(Pos destPos)
    {
        map.MoveObjectOn(destPos);
        return DelayedCall(0.5f, () => tf.position = map.WorldPos(destPos));
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
