using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Object = UnityEngine.Object;
using DG.Tweening;
using Moq;

public class MiniMapTest
{
    private ResourceLoader resourceLoader;
    private GameInfo gameInfo;

    private MiniMap miniMap;
    private MiniMap prefabMiniMap;

    private MiniMapHandlerTest prefabMiniMapHandler;
    private MiniMapHandlerTest miniMapHandler;

    private PlayerInfoTest playerInfo;
    private PlayerInfoTest prefabPlayerInfo;

    private GameObject testCanvas;
    private Camera mainCamera;

    private WorldMap map;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        gameInfo = Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));
        prefabPlayerInfo = Resources.Load<PlayerInfoTest>("Prefabs/Character/Player/PlayerInfoDummy");
        testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Canvas"));
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/UI/MainCamera"));
        prefabMiniMap = Resources.Load<MiniMap>("Prefabs/UI/MiniMap/MiniMap");
        prefabMiniMapHandler = Resources.Load<MiniMapHandlerTest>("Prefabs/Character/Player/MiniMapHandlerTest");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(resourceLoader.gameObject);
        Object.Destroy(gameInfo.gameObject);
        Object.Destroy(testCanvas.gameObject);
        Object.Destroy(mainCamera.gameObject);
    }

    [SetUp]
    public void SetUp()
    {
        map = GameInfo.Instance.Map(0);

        playerInfo = Object.Instantiate(prefabPlayerInfo, map.WorldPos(map.stairsBottom.Key), Quaternion.identity);
        miniMapHandler = Object.Instantiate(prefabMiniMapHandler, playerInfo.transform);

        var mock = new Mock<IPlayerMapUtil>();
        mock.Setup(x => x.onTilePos).Returns(map.MapPos(playerInfo.transform.position));
        playerInfo.SetMapUtil(mock.Object);

        RectTransform rectTfCanvas = testCanvas.GetComponent<RectTransform>();
        miniMap = Object.Instantiate(prefabMiniMap, rectTfCanvas);
        miniMapHandler.SetMiniMap(miniMap);
        miniMap.InitUISize(420f, 480f, 960f);

        RectTransform rectTf = miniMap.GetComponent<RectTransform>();
        rectTf.anchoredPosition = new Vector2(-260, 420);
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll();

        Object.Destroy(miniMap.gameObject);
        Object.Destroy(playerInfo.gameObject);
    }

    //[Ignore("need to implement.")]
    [UnityTest]
    public IEnumerator _001_MiniMapSetDiscoveredTest()
    {
        // setup
        Pos pos = map.stairsBottom.Key;
        yield return new WaitForSeconds(0.5f);
        miniMapHandler.OnStartFloor();
        yield return new WaitForSeconds(0.5f);
        for (Pos pointer = pos; pointer.y < map.height - 2; pointer += new Pos(0, 2))
        {
            map.miniMapData.SetDiscovered(pointer);
            miniMapHandler.UpdateMiniMap();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
