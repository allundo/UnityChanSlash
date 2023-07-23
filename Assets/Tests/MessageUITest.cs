using UniRx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;
using Moq;
using Object = UnityEngine.Object;

public class MessageUITest
{
    private ResourceLoader resourceLoader;
    private GameInfo gameInfo;
    private int secretLevel = 0;
    private TimeManager timeManager;

    private MessageControllerTest messageUI;
    private MessageControllerTest prefabMessageUI;

    private GameObject testCanvas;
    private Camera mainCamera;
    private GameObject eventSystem;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        gameInfo = Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));

        timeManager = Object.Instantiate(Resources.Load<TimeManager>("Prefabs/TimeManager"));
        timeManager.input = new Mock<IPlayerInput>().Object;

        // Load from test resources
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/UI/MainCamera"));
        eventSystem = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/EventSystem"));

        testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Canvas"));

        prefabMessageUI = Resources.Load<MessageControllerTest>("Prefabs/UI/Message/MessageUITest");

    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(testCanvas.gameObject);
        Object.Destroy(mainCamera.gameObject);
        Object.Destroy(eventSystem);
        Object.Destroy(timeManager.gameObject);
        Object.Destroy(gameInfo.gameObject);
        Object.Destroy(resourceLoader.gameObject);
    }

    [SetUp]
    public void SetUp()
    {
        messageUI = Object.Instantiate(prefabMessageUI);

        RectTransform rectTfCanvas = testCanvas.GetComponent<RectTransform>();
        RectTransform rectTf = messageUI.GetComponent<RectTransform>();
        rectTf.SetParent(rectTfCanvas);

        rectTf.anchorMin = rectTf.anchorMax = new Vector2(1f, 1f);

        secretLevel = gameInfo.secretLevel;
    }

    [TearDown]
    public void TearDown()
    {
        gameInfo.secretLevel = secretLevel;

        DOTween.KillAll();

        Object.Destroy(messageUI.gameObject);
    }
    private IEnumerator ReadMessages(BoardMessageData[] boardMessages)
        => ReadMessages(boardMessages.Select(msg => msg.Convert()));

    private IEnumerator ReadMessages(IEnumerable<MessageData> msgs)
    {
        foreach (var msg in msgs)
        {
            yield return messageUI.AutoReadMessageData(msg).ToYieldInstruction();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ReadMessages(IEnumerable<BloodMessageData> msgs)
    {
        foreach (var msg in msgs)
        {
            gameInfo.secretLevel = 0;
            yield return messageUI.AutoReadMessageData(msg).ToYieldInstruction();
            msg.isRead = false;
            gameInfo.secretLevel = 10;
            yield return messageUI.AutoReadMessageData(msg).ToYieldInstruction();
            yield return new WaitForSeconds(0.1f);
        }
    }

    [UnityTest]
    public IEnumerator _001_PlayerMessageTest()
    {
        yield return null;

        yield return messageUI.AutoReadMessageData(new MessageData(
            new MessageSource("00_デフォルト", FaceID.DEFAULT),
            new MessageSource("01_怒り1", FaceID.ANGRY),
            new MessageSource("02_にっこり", FaceID.SMILE),
            new MessageSource("03_がっかり1", FaceID.DISATTRACT),
            new MessageSource("04_気づき", FaceID.NOTICE),
            new MessageSource("05_目閉じ", FaceID.EYECLOSE),
            new MessageSource("06_怒り2", FaceID.ANGRY2),
            new MessageSource("07_がっかり2", FaceID.DISATTRACT2),
            new MessageSource("08_ジト目", FaceID.DESPISE),
            new MessageSource("09_恥ずかし", FaceID.ASHAMED),
            new MessageSource("10_びっくり", FaceID.SURPRISE),
            new MessageSource("-1_なし", FaceID.NONE)
        ), 0.25f).ToYieldInstruction();

        yield return new WaitForSeconds(0.6f);
    }

    [UnityTest]
    public IEnumerator _002_FixedBoardMessagesTest([Values(1, 2, 3, 4, 5)] int floor)
    {
        var fixedMsgs = resourceLoader.floorMessagesData.Param(floor - 1).fixedMessages;
        yield return null;

        yield return messageUI.StartCoroutine(ReadMessages(fixedMsgs));

        yield return new WaitForSeconds(1f);
    }

    [UnityTest]
    public IEnumerator _003_RandomBoardMessagesTest([Values(1, 2, 3, 4, 5)] int floor)
    {
        var randomMsgs = resourceLoader.floorMessagesData.Param(floor - 1).randomMessages;
        yield return null;

        yield return messageUI.StartCoroutine(ReadMessages(randomMsgs));

        yield return new WaitForSeconds(1f);
    }

    [UnityTest]
    public IEnumerator _004_ItemInspectMessagesTest()
    {
        List<MessageData> itemDescriptions = new List<MessageData>();

        foreach (var type in Enum.GetValues(typeof(ItemType)))
        {
            itemDescriptions.Add(MessageData.ItemDescription(new ItemInfo(resourceLoader.itemData.Param((int)type), (ItemType)type, null)));
        }

        yield return null;

        yield return messageUI.StartCoroutine(ReadMessages(itemDescriptions));

        yield return new WaitForSeconds(1f);
    }

    [UnityTest]
    public IEnumerator _005_BloodMessagesTest([Values(1, 2, 3, 4, 5)] int floor)
    {
        var bloodMsgs = resourceLoader.floorMessagesData.Param(floor - 1).bloodMessages.Select(src => new BloodMessageData(src));
        yield return null;

        yield return messageUI.StartCoroutine(ReadMessages(bloodMsgs));

        yield return new WaitForSeconds(1f);
    }

    [UnityTest]
    public IEnumerator _006_BloodMessageBranchBySecretLevelTest()
    {
        var bloodMsgs = resourceLoader.floorMessagesData.Param(2).bloodMessages.Select(src => new BloodMessageData(src)).ToArray();
        var readSecPerLiteral = 0.05f;
        yield return null;

        gameInfo.secretLevel = 0;
        messageUI.AutoReadMessageData(bloodMsgs[0], readSecPerLiteral);

        yield return new WaitForSeconds(0.6f);

        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("迷宮に魔法で刻んだ文字は残せるようだ\n不思議なことに文字を読んだ瞬間、\n記憶が戻ってきた\nこのメモはきっと迷宮攻略の鍵となる・・・", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("落書き禁止！\n\nby 迷宮の番人", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        messageUI.OnPointerUp(null);
        yield return null;

        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("・・・？", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        messageUI.OnPointerUp(null);
        yield return null;
        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("他に誰かいるのかな？\n前半と後半で筆跡もぜんぜん違う", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        Assert.AreEqual(1, gameInfo.secretLevel);

        yield return new WaitForSeconds(0.5f);

        messageUI.AutoReadMessageData(bloodMsgs[0], readSecPerLiteral);

        yield return new WaitForSeconds(0.6f);

        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("迷宮に魔法で刻んだ文字は残せるようだ\n不思議なことに文字を読んだ瞬間、\n記憶が戻ってきた\nこのメモはきっと迷宮攻略の鍵となる・・・", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("落書き禁止！\n\nby 迷宮の番人", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        // Message window is closing
        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("", messageUI.sentence);

        yield return new WaitForSeconds(0.5f);

        bloodMsgs[0].isRead = false;
        messageUI.AutoReadMessageData(bloodMsgs[0], readSecPerLiteral);

        yield return new WaitForSeconds(0.6f);

        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("迷宮に魔法で刻んだ文字は残せるようだ\n不思議なことに文字を読んだ瞬間、\n記憶が戻ってきた\nこのメモはきっと迷宮攻略の鍵となる・・・", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("落書き禁止！\n\nby 迷宮の番人", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        messageUI.OnPointerUp(null);
        yield return null;

        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("前半と後半で筆跡がぜんぜん違う", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        messageUI.OnPointerUp(null);
        yield return null;

        Assert.AreEqual("", messageUI.title);
        Assert.AreEqual("なんかケンカしてるっぽい・・・", messageUI.sentence);

        yield return new WaitForSeconds(messageUI.sentence.Length * readSecPerLiteral);

        Assert.AreEqual(1, gameInfo.secretLevel);
    }
}
