using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EndingTest
{
    private EndingUIHandler endingUIHandler;
    private EndingSceneMediator endingSceneMediator;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));
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
        Object.Destroy(endingUIHandler);
    }

    [UnityTest]
    public IEnumerator _001_NoonEndingTest()
    {
        // Setup
        GameInfo.Instance.clearTimeSec = 2000;

        // When
        endingSceneMediator = Object.Instantiate(Resources.Load<EndingSceneMediator>("Prefabs/Ending/EndingSceneMediator"));
        endingSceneMediator.endingUIHandler = endingUIHandler;
        yield return new WaitForSeconds(4f);

        // Then
        Assert.AreEqual("昼", endingUIHandler.periodType);

        // TearDown
        Object.Destroy(endingSceneMediator);
    }

    [UnityTest]
    public IEnumerator _002_EveningEndingTest()
    {
        // Setup
        GameInfo.Instance.clearTimeSec = 6000;

        // When
        endingSceneMediator = Object.Instantiate(Resources.Load<EndingSceneMediator>("Prefabs/Ending/EndingSceneMediator"));
        endingSceneMediator.endingUIHandler = endingUIHandler;
        yield return new WaitForSeconds(4f);

        // Then
        Assert.AreEqual("夕", endingUIHandler.periodType);

        // TearDown
        Object.Destroy(endingSceneMediator);
    }

    [UnityTest]
    public IEnumerator _003_NightEndingTest()
    {
        // Setup
        GameInfo.Instance.clearTimeSec = 8000;

        // When
        endingSceneMediator = Object.Instantiate(Resources.Load<EndingSceneMediator>("Prefabs/Ending/EndingSceneMediator"));
        endingSceneMediator.endingUIHandler = endingUIHandler;
        yield return new WaitForSeconds(4f);

        // Then
        Assert.AreEqual("夜", endingUIHandler.periodType);

        // TearDown
        Object.Destroy(endingSceneMediator);
    }

    [UnityTest]
    public IEnumerator _004_MorningEndingTest()
    {
        // Setup
        GameInfo.Instance.clearTimeSec = 80000;

        // When
        endingSceneMediator = Object.Instantiate(Resources.Load<EndingSceneMediator>("Prefabs/Ending/EndingSceneMediator"));
        endingSceneMediator.endingUIHandler = endingUIHandler;
        yield return new WaitForSeconds(4f);

        // Then
        Assert.AreEqual("朝", endingUIHandler.periodType);

        // TearDown
        Object.Destroy(endingSceneMediator);
    }
}
