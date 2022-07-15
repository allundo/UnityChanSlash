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

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/Result/ResultCamera"));
        ground = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Result/Ground"));

        var defaultPos = mainCamera.transform.position;
        mainCamera.transform.position = new Vector3(0, defaultPos.y, defaultPos.z);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(mainCamera.gameObject);
        Object.Destroy(ground);
        Object.Destroy(resourceLoader.gameObject);
    }

    [SetUp]
    public void SetUp()
    {
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll();
    }

    [UnityTest]
    public IEnumerator _001_MoneyBagDropTest()
    {
        var prefabSphere = Resources.Load<SphereCollider>("Prefabs/Result/TargetSphere");
        var headCollider = Object.Instantiate(prefabSphere, new Vector3(0, 0.05f, 0.6f), Quaternion.identity);
        var footCollider = Object.Instantiate(prefabSphere, new Vector3(0, 0.05f, -0.6f), Quaternion.identity);

        yield return null;

        var bag = new BagControl(10000001);
        bag.SetPressTarget(new ClothSphereColliderPair(headCollider, footCollider));

        yield return new WaitForSeconds(2f);

        bag.Drop().Play();

        yield return new WaitForSeconds(8f);

        // TearDown
        Object.Destroy(headCollider);
        Object.Destroy(footCollider);
        bag.Destroy();
    }
}
