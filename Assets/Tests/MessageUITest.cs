using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;
using Moq;

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

    [UnityTest]
    public IEnumerator _001_PlayerMessageTest()
    {
        yield return null;
        messageUI.InputMessageData(new MessageData[]
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
        });

        yield return new WaitForSeconds(10f);
    }
}
