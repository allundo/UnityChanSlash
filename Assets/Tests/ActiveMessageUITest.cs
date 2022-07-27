using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;
using Moq;

public class ActiveMessageUITest
{
    private ActiveMessageController messageUI;

    private GameObject testCanvas;
    private Camera mainCamera;
    private GameObject eventSystem;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {

        // Load from test resources
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/UI/MainCamera"));
        eventSystem = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/EventSystem"));

        testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Canvas"));
        RectTransform rectTfCanvas = testCanvas.GetComponent<RectTransform>();

        messageUI = Object.Instantiate(Resources.Load<ActiveMessageController>("Prefabs/UI/Message/ActiveMessageUI"), rectTfCanvas);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(testCanvas.gameObject);
        Object.Destroy(mainCamera.gameObject);
        Object.Destroy(eventSystem);
        Object.Destroy(messageUI.gameObject);
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll();
    }

    private IEnumerator ReadMessageData(ActiveMessageData[] data, float readTime = 4f)
    {
        yield return null;

        foreach (var mes in data)
        {
            messageUI.InputMessageData(mes);
            var duration = mes.sentence.Length / mes.literalsPerSec;
            yield return new WaitForSeconds(readTime);

            yield return null;

            if (duration > readTime)
            {
                yield return new WaitForSeconds(readTime);
                messageUI.Close();
            }
        }
    }

    [UnityTest]
    public IEnumerator _001_ActiveMessagesFaceAndEmotionTest()
    {
        yield return null;

        yield return messageUI.StartCoroutine(ReadMessageData(new ActiveMessageData[]
        {
            new ActiveMessageData("00_デフォルト, -1_なし", SDFaceID.DEFAULT, SDEmotionID.NONE),
            new ActiveMessageData("01_怒り1, 00_！", SDFaceID.ANGRY, SDEmotionID.SURPRISE),
            new ActiveMessageData("02_怒り2, 01_？", SDFaceID.ANGRY2, SDEmotionID.QUESTION),
            new ActiveMessageData("03_がっかり, 02_！！", SDFaceID.DISATTRACT, SDEmotionID.EXSURPRISE),
            new ActiveMessageData("04_目閉じ, 03_！？", SDFaceID.EYECLOSE, SDEmotionID.EXQUESTION),
            new ActiveMessageData("05_悲しい1, 04_ため息", SDFaceID.SAD, SDEmotionID.SIGH),
            new ActiveMessageData("06_悲しい2, 05_イライラ", SDFaceID.SAD2, SDEmotionID.IRRITATE),
            new ActiveMessageData("07_にっこり, 06_わいわい", SDFaceID.SMILE, SDEmotionID.WAIWAI),
            new ActiveMessageData("08_びっくり, 07_ブルー", SDFaceID.SURPRISE, SDEmotionID.BLUE),
            new ActiveMessageData("00_デフォルト, 08_不満", SDFaceID.DEFAULT, SDEmotionID.CONFUSE),
        }, 4f));

        yield return new WaitForSeconds(0.6f);
    }
}
