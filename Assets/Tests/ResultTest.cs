using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;

public class ResultTest
{
    private ResourceLoader resourceLoader;
    private Camera mainCamera;
    private GroundCoinGenerator generator;
    private UnityChanResultReactor unityChanReactor;
    private Light directionalLight;
    private ResultSpotLight spotLight;
    private GameObject ceil;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/Result/ResultCamera"));
        generator = Object.Instantiate(Resources.Load<GroundCoinGenerator>("Prefabs/Result/GroundCoinGenerator"));
        ceil = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Result/Ceil"));
        directionalLight = Object.Instantiate(Resources.Load<Light>("Prefabs/Result/ResultDirectionalLight"));

        var defaultPos = mainCamera.transform.position;
        mainCamera.transform.position = new Vector3(0, defaultPos.y, defaultPos.z);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(generator.gameObject);
        Object.Destroy(directionalLight.gameObject);
        Object.Destroy(ceil);
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
        generator.DestroyAll();
        DOTween.KillAll();
        Object.Destroy(spotLight);
        Object.Destroy(unityChanReactor.gameObject);
    }

    [UnityTest]
    public IEnumerator _001_GiantMoneyBagDropTest()
    {
        // setup
        var bag = new BagControl(10000500, generator);
        var handler = new ResultCharactersHandler(unityChanReactor, spotLight, bag);

        yield return new WaitForSeconds(4f);

        // when
        handler.StartAction();

        // then
        // Check visual by your own eyes
        Assert.AreEqual(BagSize.Gigantic, bag.bagSize);
        Assert.AreEqual(1, bag.surplusCoins);

        yield return new WaitForSeconds(8f);

        float sqrMaxDistance = (0.85f * 0.85f) * 3f;

        bag.bagTf.ForEach(child =>
        {
            if ("Big500yen(Clone)" == child.gameObject.name)
            {
                Assert.Less((child.transform.position - bag.bagTf.position).sqrMagnitude, sqrMaxDistance, "A coin is out of the bag.");
            }
        });

        // tear down
        bag.Destroy();
    }

    [UnityTest]
    public IEnumerator _002_SmallMoneyBagDropTest()
    {
        // setup
        var bag = new BagControl(500000, null);
        var handler = new ResultCharactersHandler(unityChanReactor, spotLight, bag);

        yield return new WaitForSeconds(4f);

        // when
        handler.StartAction();

        // then
        // Check visual by your own eyes
        Assert.AreEqual(BagSize.Small, bag.bagSize);

        yield return new WaitForSeconds(8f);

        float sqrMaxDistance = (0.1f * 0.1f) * 3f;

        bag.bagTf.ForEach(child =>
        {
            if ("Big500yen(Clone)" == child.gameObject.name)
            {
                Assert.Less((child.transform.position - bag.bagTf.position).sqrMagnitude, sqrMaxDistance, "A coin is out of the bag.");
            }
        });

        // tear down
        bag.Destroy();
    }

    [UnityTest]
    public IEnumerator _003_MiddleMoneyBagDropTest()
    {
        // setup
        var bag = new BagControl(500001, null);
        var handler = new ResultCharactersHandler(unityChanReactor, spotLight, bag);

        yield return new WaitForSeconds(4f);

        // when
        handler.StartAction();

        // then
        // Check visual by your own eyes
        Assert.AreEqual(BagSize.Middle, bag.bagSize);

        yield return new WaitForSeconds(8f);

        float sqrMaxDistance = (0.25f * 0.25f) * 3f;

        bag.bagTf.ForEach(child =>
        {
            if ("Big500yen(Clone)" == child.gameObject.name)
            {
                Assert.Less((child.transform.position - bag.bagTf.position).sqrMagnitude, sqrMaxDistance, "A coin is out of the bag.");
            }
        });

        // tear down
        bag.Destroy();
    }
    [UnityTest]
    public IEnumerator _004_BigMoneyBagDropTest()
    {
        // setup
        var bag = new BagControl(2000001, null);
        var handler = new ResultCharactersHandler(unityChanReactor, spotLight, bag);

        yield return new WaitForSeconds(4f);

        // when
        handler.StartAction();

        // then
        // Check visual by your own eyes
        Assert.AreEqual(BagSize.Big, bag.bagSize);

        yield return new WaitForSeconds(8f);

        float sqrMaxDistance = (0.5f * 0.5f) * 3f;

        bag.bagTf.ForEach(child =>
        {
            if ("Big500yen(Clone)" == child.gameObject.name)
            {
                Assert.Less((child.transform.position - bag.bagTf.position).sqrMagnitude, sqrMaxDistance, "A coin is out of the bag.");
            }
        });

        yield return new WaitForSeconds(4f);

        // tear down
        bag.Destroy();
    }
}
