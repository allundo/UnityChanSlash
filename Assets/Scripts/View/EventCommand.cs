using UnityEngine;
using DG.Tweening;

public class EventCommand : Command
{
    protected PlayerInput playerInput;
    protected MessageController messageController;
    protected GameOverUI gameOverUI;
    protected LightManager lightManager;

    public EventCommand(EventCommandTarget target, float duration = 1f) : base(target, duration, 0.95f)
    {
        playerInput = target.input as PlayerInput;
        messageController = target.messageController;
        gameOverUI = target.gameOverUI;
        lightManager = target.lightManager;
    }

    protected void SetUIInvisible(bool isVisibleOnCompleted = true)
    {
        playerInput.SetInputVisible(false);
        SetOnCompleted(() => playerInput.SetInputVisible(isVisibleOnCompleted));
    }
}

public class EventMessage : EventCommand
{
    protected MessageData[] data;
    protected bool isUIVisibleOnCompleted;

    public EventMessage(EventCommandTarget target, MessageData[] data, bool isUIVisibleOnCompleted = true, bool isWaitForCommand = true) : base(target)
    {
        this.data = data;
        this.isUIVisibleOnCompleted = isUIVisibleOnCompleted;
        if (isWaitForCommand) duration += playerInput.RemainingDuration;
    }

    protected override bool Action()
    {
        if (data == null) return false;
        SetUIInvisible(isUIVisibleOnCompleted);
        messageController.InputMessageData(data);
        return true;
    }
}

public class GameOverEvent : EventCommand
{
    public GameOverEvent(EventCommandTarget target) : base(target) { }

    protected override bool Action()
    {
        gameOverUI.Play();
        return true;
    }
}

public class WitchGenerateEvent : EventCommand
{
    private IDirection witchDir;

    public WitchGenerateEvent(EventCommandTarget target, IDirection witchDir) : base(target, 300f)
    {
        this.witchDir = witchDir;
    }

    protected override bool Action()
    {
        Sequence seq = DOTween.Sequence();
        Pos witchPos = map.GetForward;
        float interval = 0.5f;

        if (map.dir.IsInverse(witchDir))
        {
            seq
                .AppendCallback(() => playerInput.EnqueueTurnL())
                .AppendCallback(() => playerInput.EnqueueTurnL());

            witchPos = map.GetBackward;

            interval += 0.6f;
        }
        else if (map.dir.IsLeft(witchDir))
        {
            seq.AppendCallback(() => playerInput.EnqueueTurnL());
            witchPos = map.GetLeft;
            interval += 0.3f;
        }
        else if (map.dir.IsRight(witchDir))
        {
            seq.AppendCallback(() => playerInput.EnqueueTurnR());
            witchPos = map.GetRight;
            interval += 0.3f;
        }

        playingTween = seq
            .InsertCallback(0.5f, () => GameManager.Instance.PlaceWitch(witchPos, witchDir.Backward, 300f))
            .Append(lightManager.DirectionalFade(1f, 0.2f, 1.0f))
            .Join(lightManager.SpotFadeIn(map.WorldPos(witchPos) + new Vector3(0, 4f, 0), 1f, 30f, 1.0f))
            .AppendInterval(interval)
            .Append(lightManager.DirectionalFade(0.2f, 1f, 1.5f))
            .Join(lightManager.SpotFadeOut(30f, 1f))
            .AppendCallback(() => messageController.InputMessageData(
                new MessageData[]
                {
                    new MessageData("『表の立て札は読まなかったのかい？』", FaceID.NONE),
                    new MessageData("いやまあ読んだけど・・・\n誰よ？あんた", FaceID.DEFAULT),
                    new MessageData("『私は迷宮の守護霊。その鍵は返してもらう。』", FaceID.NONE),
                    new MessageData("こっちだってコレないと外に出られないんだけど？？", FaceID.DESPISE),
                    new MessageData("『そっちの事情なんて知らないね。私はここの宝物を守るように命令を受けている。』", FaceID.NONE),
                    new MessageData("『ここで逃がすわけにはいかない・・・！』", FaceID.NONE),
                    new MessageData("ふーん・・・", FaceID.ASHAMED),
                    new MessageData("それ、誰の命令なんだろうね？", FaceID.EYECLOSE),
                    new MessageData("『誰・・・。誰って・・・・？』", FaceID.NONE),
                    new MessageData("知らないんだ？", FaceID.DISATTRACT2),
                    new MessageData("・・・まあ、私だってそっちの事情なんて知らんし", FaceID.DEFAULT),
                    new MessageData("こんなとこ、とっととトンズラさせてもらうわ！", FaceID.ANGRY),
                }
            ))
            .SetUpdate(false)
            .Play();

        SetOnCompleted(() => playerInput.SetInputVisible());

        return true;
    }
}