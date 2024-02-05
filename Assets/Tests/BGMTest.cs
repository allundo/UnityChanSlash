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

    private ResourceLoader resourceLoader;
    private GameObject testCanvas;
    private ItemInventoryTest prefabItemInventory;
    private ItemInventoryTest itemInventory;
    private ItemIconGenerator itemIconGenerator;

    private ActiveMessageController activeMessageUI;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        prefabBGMManager = Resources.Load<BGMManager>("Prefabs/System/BGMManager");
        audioListener = new GameObject("AudioListener").AddComponent<AudioListener>();

        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Canvas"));
        prefabItemInventory = Resources.Load<ItemInventoryTest>("Prefabs/UI/Item/ItemInventoryTest");
        itemIconGenerator = Object.Instantiate(Resources.Load<ItemIconGenerator>("Prefabs/UI/Item/ItemIconGenerator"));

        itemInventory = Object.Instantiate(prefabItemInventory);
        InitItemInventory(itemInventory);

        activeMessageUI = Object.Instantiate(Resources.Load<ActiveMessageController>("Prefabs/UI/Message/ActiveMessageUI"), testCanvas.transform);
        activeMessageUI.ResetOrientation(DeviceOrientation.Portrait);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        itemIconGenerator.DestroyAll();
        Object.Destroy(itemIconGenerator.gameObject);
        Object.Destroy(itemInventory.gameObject);
        Object.Destroy(activeMessageUI.gameObject);
        Object.Destroy(testCanvas.gameObject);
        Object.Destroy(resourceLoader.gameObject);
        Object.Destroy(audioListener.gameObject);
    }

    private void InitItemInventory(ItemInventoryTest itemInventory)
    {
        RectTransform rectTfCanvas = testCanvas.GetComponent<RectTransform>();
        RectTransform rectTf = itemInventory.GetComponent<RectTransform>();
        rectTf.SetParent(rectTfCanvas);

        Vector2 size = rectTf.sizeDelta;
        rectTf.anchorMin = rectTf.anchorMax = new Vector2(0f, 0.5f);
        rectTf.anchoredPosition = new Vector2(size.x, size.y + 280f) * 0.5f;

        itemIconGenerator.SetParent(itemInventory.transform);
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

    private void SetWitchInfo(bool isLiving)
    {
        var mock = new Mock<IWitchInfo>();
        mock.Setup(w => w.IsWitchLiving()).Returns(isLiving);
        bgmManager.SetWitchInfo(mock.Object);
    }

    [UnityTest]
    public IEnumerator _001_FloorBGMPlayTest([Values(1, 2, 3, 4, 5)] int floor)
    {
        yield return null;

        SetWitchInfo(false);
        bgmManager.LoadFloor(floor);

        yield return null;

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(3f);
    }

    [UnityTest]
    public IEnumerator _002_FloorBGMChangeTest()
    {
        yield return null;

        SetWitchInfo(false);
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
        yield return null;

        SetWitchInfo(false);
        bgmManager.LoadFloor(5);

        yield return null;

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(3f);

        bgmManager.SwitchBossBGM();
        itemInventory.PickUp(resourceLoader.ItemInfo(ItemType.KeyBlade));

        yield return new WaitForSeconds(5f);

        bgmManager.ReserveBackToFloorBGM();

        yield return new WaitForSeconds(15f);

        bgmManager.SwitchBossBGM();

        yield return new WaitForSeconds(5f);

        bgmManager.SwitchFloor(4);

        yield return new WaitForSeconds(1f);

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(5f);

        var dummyKeyBlade = itemIconGenerator.Spawn(Vector2.zero, resourceLoader.ItemInfo(ItemType.KeyBlade, 1)).SetIndex(0);
        itemInventory.Remove(dummyKeyBlade);
        SetWitchInfo(false);

        bgmManager.SwitchFloor(3);

        yield return new WaitForSeconds(1f);

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(5f);

        SetWitchInfo(true);
        bgmManager.SwitchFloor(4);

        yield return new WaitForSeconds(1f);

        bgmManager.PlayFloorBGM();

        yield return new WaitForSeconds(5f);
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
