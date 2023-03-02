using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;
using Moq;
using UniRx;

public class RankRecordTest
{
    private DeadRecord prefabDeadRecord;
    private RankInMessage prefabRankInMessage;

    private GameObject testCanvas;
    private Camera mainCamera;
    private GameObject eventSystem;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        prefabDeadRecord = Resources.Load<DeadRecord>("Prefabs/UI/Ranking/DeadRecord");
        prefabRankInMessage = Resources.Load<RankInMessage>("Prefabs/UI/Ranking/RankInMessage");

        // Load from test resources
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/UI/MainCamera"));
        eventSystem = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/EventSystem"));

        testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Canvas"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(testCanvas.gameObject);
        Object.Destroy(mainCamera.gameObject);
        Object.Destroy(eventSystem);
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll();
    }

    [UnityTest]
    public IEnumerator _001_RankEffectTest()
    {
        // Setup
        var deadRecord = Object.Instantiate(prefabDeadRecord, testCanvas.transform);
        var rankInMessage = Object.Instantiate(prefabRankInMessage, testCanvas.transform);
        var offset = new Vector2(0f, -Screen.height * 0.5f);

        yield return null;

        for (int rank = 0; rank < 11; rank++)
        {
            var rankRatio = rank > 0 ? 11 - rank : 0;
            deadRecord.SetValues(rank, rank % 5, rankRatio * 1000000, "テスト死因");

            deadRecord.ResetPosition(offset);

            var seq = DOTween.Sequence()
                .AppendCallback(() => deadRecord.SetRankEnable(false))
                .Append(deadRecord.SlideInTween())
                .Append(deadRecord.RankEffect(rank))
                .Append(deadRecord.RankPunchEffect(rank));

            if (rank > 0)
            {
                rankInMessage.ResetOrientation(DeviceOrientation.Portrait);
                seq.Append(rankInMessage.RankInTween());
            }

            seq.Play();

            yield return new WaitForSeconds(seq.Duration() + 0.25f);
        }
        yield return null;

        Object.Destroy(deadRecord.gameObject);
        Object.Destroy(rankInMessage.gameObject);
    }
}
