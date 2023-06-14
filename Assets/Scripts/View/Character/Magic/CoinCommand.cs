using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class CoinCommand : MagicCommand
{
    protected CoinInput coinInput;

    public CoinCommand(ICommandTarget target, float duration) : base(target, duration)
    {
        coinInput = target.input as CoinInput;
    }
}
public class CoinMove : CoinCommand
{
    protected IMortalReactor mortalReact;
    protected CoinBound coinBound;

    public CoinMove(ICommandTarget target, float duration) : base(target, duration)
    {
        mortalReact = react as IMortalReactor;
        coinBound = new CoinBound(target, duration);
    }

    protected virtual Tween AttackSequence => attack.AttackSequence(duration);

    public override IObservable<Unit> Execute()
    {
        // Forward movable?
        if (map.ForwardTile.IsViewOpen)
        {
            if (mortalReact.CurrentHP <= 1f)
            {
                map.MoveObjectOn(map.GetForward);
                playingTween = tweenMove.JumpHalf(map.DestVec - new Vector3(0, 1f, 0), 0.75f, false).Play();
                completeTween = AttackSequence.Play();
                target.input.Interrupt(coinBound, false, true);
                return ObservableComplete(0.75f);
            }

            mortalReact.ReduceHP();

            playingTween = MoveForward();
            completeTween = AttackSequence.Play();
            validateTween = ValidateTween().Play();
            return ObservableComplete();
        }
        else
        {
            playingTween = tweenMove.MoveRelative(map.GetForwardVector() * 0.75f).Play();

            validateTween = tweenMove.DelayedCall(0.75f, () => coinInput.InputDrop()).Play();
            return ObservableComplete(0.75f);
        }
    }
}

public class CoinFire : CoinMove
{
    public CoinFire(ICommandTarget target, float duration) : base(target, duration) { }

    // Enable attack collider after a half duration
    protected override Tween AttackSequence => attack.AttackSequence(duration * 0.5f).SetDelay(duration * 0.5f);
}

public class CoinBound : CoinCommand
{
    public CoinBound(ICommandTarget target, float duration) : base(target, duration) { }

    public override IObservable<Unit> Execute()
    {
        // Forward movable?
        if (map.ForwardTile.IsViewOpen)
        {
            map.MoveObjectOn(map.GetForward);
            playingTween = tweenMove.JumpHalf(map.DestVec * 0.5f + new Vector3(0f, 0.4f, 0), 0.5f).Play();
            validateTween = tweenMove.DelayedCall(0.5f, () => coinInput.InputDrop()).Play();

            return ObservableComplete(0.5f);
        }
        else
        {
            playingTween = tweenMove.JumpHalf(map.GetForwardVector() * 0.75f + new Vector3(0f, 0.5f, 0), 0.75f).Play();
            validateTween = tweenMove.DelayedCall(0.75f, () => coinInput.InputDrop()).Play();

            return ObservableComplete(0.75f);
        }
    }
}

public class CoinDrop : CoinCommand
{
    protected Item coin;

    public CoinDrop(ICommandTarget target, float duration) : base(target, duration) { }

    public CoinDrop SetCoin(Item coin)
    {
        this.coin = coin;
        coin?.SetDisplay(false);
        return this;
    }
    public override IObservable<Unit> Execute()
    {
        var dropVec = map.DestVec * 0.5f - new Vector3(0, 1f + map.CurrentVec3Pos.y, 0);

        var seq = DOTween.Sequence()
            .Append(tweenMove.JumpHalf(dropVec, 0.5f, false))
            .AppendCallback(react.OnDie);

        if (coin != null)
        {
            seq.Append(tweenMove.JumpRelative(map.DestVec * 0.5f, 0.5f, 0.5f))
                .AppendCallback(() => coin.SetDisplay());
        }
        else
        {
            seq.Append(tweenMove.MoveRelative(new Vector3(0, -1f, 0), 0.5f));
        }

        playingTween = seq.SetUpdate(false).Play();

        return ObservableComplete(); // Don't validate input.
    }
}