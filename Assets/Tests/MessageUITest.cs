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

        timeManager = Object.Instantiate(Resources.Load<TimeManager>("Prefabs/TimeManager"));
        var playerTargetMock = new Mock<ICommandTarget>();
        playerTargetMock.Setup(m => m.input).Returns(new Mock<IPlayerInput>().Object);
        timeManager.target = playerTargetMock.Object;

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
    }

    [TearDown]
    public void TearDown()
    {
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
        var bloodMsgs = resourceLoader.floorMessagesData.Param(floor - 1).bloodMessages;
        yield return null;

        yield return messageUI.StartCoroutine(ReadMessages(bloodMsgs));

        yield return new WaitForSeconds(1f);
    }
}
