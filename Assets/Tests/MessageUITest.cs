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

    private MessageController messageUI;
    private MessageController prefabMessageUI;

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

        prefabMessageUI = Resources.Load<MessageController>("Prefabs/UI/Message/MessageUI");

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

    private IEnumerator ReadMessages(IEnumerable<MessageData[]> msgs)
    {
        foreach (var msg in msgs)
        {
            yield return messageUI.StartCoroutine(ReadMessageData(msg));
            yield return new WaitForSeconds(0.6f);
        }
    }

    private IEnumerator ReadMessageData(MessageData[] data, float readSecPerLiteral = 0.1f)
    {
        messageUI.InputMessageData(data);
        yield return null;
        timeManager.Resume(false);
        yield return new WaitForSeconds(0.5f);

        foreach (var mes in data)
        {
            var duration = mes.sentence.Length / mes.literalsPerSec;
            var readTime = mes.sentence.Length * readSecPerLiteral;
            yield return new WaitForSeconds(readTime);

            messageUI.OnPointerUp(null);
            yield return null;

            // Full sentence is displayed by a tap simulation.
            if (duration > readTime)
            {
                yield return new WaitForSeconds(readTime);
                // Tap to next message.
                messageUI.OnPointerUp(null);
            }
        }
    }

    [UnityTest]
    public IEnumerator _001_PlayerMessageTest()
    {
        yield return null;

        yield return messageUI.StartCoroutine(ReadMessageData(new MessageData[]
        {
            new MessageData("00_デフォルト", FaceID.DEFAULT),
            new MessageData("01_怒り1", FaceID.ANGRY),
            new MessageData("02_にっこり", FaceID.SMILE),
            new MessageData("03_がっかり1", FaceID.DISATTRACT),
            new MessageData("04_気づき", FaceID.NOTICE),
            new MessageData("05_目閉じ", FaceID.EYECLOSE),
            new MessageData("06_怒り2", FaceID.ANGRY2),
            new MessageData("07_がっかり2", FaceID.DISATTRACT2),
            new MessageData("08_ジト目", FaceID.DESPISE),
            new MessageData("09_恥ずかし", FaceID.ASHAMED),
            new MessageData("10_びっくり", FaceID.SURPRISE),
            new MessageData("-1_なし", FaceID.NONE),
        }, 0.25f));

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
        List<MessageData[]> itemDescriptions = new List<MessageData[]>();

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
