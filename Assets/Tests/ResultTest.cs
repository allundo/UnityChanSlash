using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;

public class ResultTest
{
    private ResourceLoader resourceLoader;
    private Camera mainCamera;
    private GameObject ground;
    private UnityChanResultReactor unityChanReactor;
    private Light directionalLight;
    private ResultSpotLight spotLight;
    private GameObject ceil;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/Result/ResultCamera"));
        ground = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Result/Ground"));
        ceil = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Result/Ceil"));
        directionalLight = Object.Instantiate(Resources.Load<Light>("Prefabs/Result/ResultDirectionalLight"));

        var defaultPos = mainCamera.transform.position;
        mainCamera.transform.position = new Vector3(0, defaultPos.y, defaultPos.z);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(directionalLight);
        Object.Destroy(ceil);
        Object.Destroy(ground);
        Object.Destroy(mainCamera.gameObject);
        Object.Destroy(resourceLoader.gameObject);
    }

    [SetUp]
    public void SetUp()
    {
        unityChanReactor = Object.Instantiate(Resources.Load<UnityChanResultReactor>("Prefabs/Result/unitychan_result"));
        spotLight = Object.Instantiate(Resources.Load<ResultSpotLight>("Prefabs/Result/ResultSpotLight"));
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll();
        Object.Destroy(spotLight);
        Object.Destroy(unityChanReactor.gameObject);
    }

    [UnityTest]
    public IEnumerator _001_GiantMoneyBagDropTest()
    {
        // setup
        var bag = new BagControl(10000500);
        var handler = new ResultCharactersHandler(unityChanReactor, spotLight, bag);

        yield return new WaitForSeconds(4f);

        // when
        handler.StartAction();

        // then
        // Check visual by your own eyes
        Assert.AreEqual(BagSize.Gigantic, bag.bagSize);
        Assert.AreEqual(1, bag.surplusCoins);

        yield return new WaitForSeconds(8f);

        // tear down
        bag.Destroy();
    }

    [UnityTest]
    public IEnumerator _002_SmallMoneyBagDropTest()
    {
        // setup
        var bag = new BagControl(500000);
        var handler = new ResultCharactersHandler(unityChanReactor, spotLight, bag);

        yield return new WaitForSeconds(4f);

        // when
        handler.StartAction();

        // then
        // Check visual by your own eyes
        Assert.AreEqual(BagSize.Small, bag.bagSize);

        yield return new WaitForSeconds(8f);

        // tear down
        bag.Destroy();
    }

    [UnityTest]
    public IEnumerator _003_MiddleMoneyBagDropTest()
    {
        // setup
        var bag = new BagControl(500001);
        var handler = new ResultCharactersHandler(unityChanReactor, spotLight, bag);

        yield return new WaitForSeconds(4f);

        // when
        handler.StartAction();

        // then
        // Check visual by your own eyes
        Assert.AreEqual(BagSize.Middle, bag.bagSize);

        yield return new WaitForSeconds(8f);

        // tear down
        bag.Destroy();
    }
}
