using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;
using Moq;
using UniRx;

public class BGMTest
{
    private BGMManager bgmManager;
    private BGMManager prefabBGMManager;
    private AudioListener audioListener;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        prefabBGMManager = Resources.Load<BGMManager>("Prefabs/System/BGMManager");
        audioListener = new GameObject("AudioListener").AddComponent<AudioListener>();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(audioListener.gameObject);
    }

    [SetUp]
    public void SetUp()
    {
        bgmManager = Object.Instantiate(prefabBGMManager);
    }

    [TearDown]
    public void TearDown()
    {
        bgmManager.Stop();
        Object.Destroy(bgmManager.gameObject);
    }

    [UnityTest]
    public IEnumerator _001_FloorBGMPlayTest([Values(1, 2, 3, 4, 5)] int floor)
    {
        // Setup
        yield return null;

        bgmManager.LoadFloor(floor);

        yield return null;

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(3f);
    }

    [UnityTest]
    public IEnumerator _002_FloorBGMChangeTest()
    {
        // Setup
        yield return null;

        bgmManager.LoadFloor(2);

        yield return null;

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(3f);

        bgmManager.SwitchFloor(3);

        yield return new WaitForSeconds(2f);

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(3f);

        bgmManager.SwitchFloor(2);

        yield return new WaitForSeconds(2f);

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(3f);
    }

    [UnityTest]
    public IEnumerator _003_WitchBGMChangeTest()
    {
        // Setup
        yield return null;

        bgmManager.LoadFloor(5);

        yield return null;

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(3f);

        bgmManager.SwitchBossBGM();

        yield return new WaitForSeconds(5f);

        bgmManager.ReserveBackToFloorBGM();

        yield return new WaitForSeconds(15f);

        bgmManager.SwitchBossBGM();

        yield return new WaitForSeconds(15f);
    }

    [UnityTest]
    public IEnumerator _004_AudioLoopSourcePausingTest()
    {
        BGMType ruin = BGMType.Ruin;
        var audio = new GameObject(ruin.ToString());
        var src = audio.AddComponent<AudioLoopSource>();
        src.LoadClips(ruin);

        yield return null;

        src.Play();

        yield return new WaitForSeconds(5f);

        src.Pause();

        Debug.Log("Wait for reserved audio loop clip: 15sec");
        yield return new WaitForSeconds(15f);

        src.UnPause();

        yield return new WaitForSeconds(10f);

        src.DestroyByHandler();
        Object.Destroy(audio);
    }
}
