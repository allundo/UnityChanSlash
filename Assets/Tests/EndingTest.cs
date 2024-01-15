using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;

public class EndingTest
{
    private ResourceLoader resourceLoader;
    private GameInfo gameInfo;
    private AudioListener audioListener;

    private EndingUIHandler endingUIHandler;
    private EndingSceneMediator endingSceneMediator;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        gameInfo = Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));
        audioListener = new GameObject("AudioListener").AddComponent<AudioListener>();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(audioListener.gameObject);
        Object.Destroy(resourceLoader.gameObject);
        Object.Destroy(gameInfo.gameObject);
    }

    [SetUp]
    public void SetUp()
    {
        GameInfo.Instance.startActionID = 0;
        GameInfo.Instance.isScenePlayedByEditor = false;

        endingUIHandler = Object.Instantiate(Resources.Load<EndingUIHandler>("Prefabs/Ending/EndingUIRegion"));
    }

    [TearDown]
    public void TearDown()
    {
        BGMManager.Instance.Stop();
        DOTween.KillAll();
        Object.Destroy(endingSceneMediator.gameObject);
        Object.Destroy(endingUIHandler.gameObject);
    }

    [UnityTest]
    public IEnumerator _001_NoonEndingTest()
    {
        // Setup
        GameInfo.Instance.endTimeSec = 2000;

        // When
        endingSceneMediator = Object.Instantiate(Resources.Load<EndingSceneMediator>("Prefabs/Ending/EndingSceneMediator"));
        endingSceneMediator.endingUIHandler = endingUIHandler;
        yield return new WaitForSeconds(4f);

        // Then
        Assert.AreEqual("昼", endingUIHandler.periodType);
    }

    [UnityTest]
    public IEnumerator _002_EveningEndingTest()
    {
        // Setup
        GameInfo.Instance.endTimeSec = 6000;

        // When
        endingSceneMediator = Object.Instantiate(Resources.Load<EndingSceneMediator>("Prefabs/Ending/EndingSceneMediator"));
        endingSceneMediator.endingUIHandler = endingUIHandler;
        yield return new WaitForSeconds(4f);

        // Then
        Assert.AreEqual("夕", endingUIHandler.periodType);
    }

    [UnityTest]
    public IEnumerator _003_NightEndingTest()
    {
        // Setup
        GameInfo.Instance.endTimeSec = 8000;

        // When
        endingSceneMediator = Object.Instantiate(Resources.Load<EndingSceneMediator>("Prefabs/Ending/EndingSceneMediator"));
        endingSceneMediator.endingUIHandler = endingUIHandler;
        yield return new WaitForSeconds(4f);

        // Then
        Assert.AreEqual("夜", endingUIHandler.periodType);
    }

    [UnityTest]
    public IEnumerator _004_MorningEndingTest()
    {
        // Setup
        GameInfo.Instance.endTimeSec = 80000;

        // When
        endingSceneMediator = Object.Instantiate(Resources.Load<EndingSceneMediator>("Prefabs/Ending/EndingSceneMediator"));
        endingSceneMediator.endingUIHandler = endingUIHandler;
        yield return new WaitForSeconds(4f);

        // Then
        Assert.AreEqual("朝", endingUIHandler.periodType);
    }
}
