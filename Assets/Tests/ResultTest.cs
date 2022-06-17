using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DG.Tweening;

public class ResultTest
{
    private Camera mainCamera;
    private GameObject ground;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
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
        var bag = Object.Instantiate(Resources.Load<BagControl>("Prefabs/Result/BagControls"), new Vector3(0, 5f, 0f), Quaternion.identity);
        var target = Object.Instantiate(Resources.Load<CapsuleCollider>("Prefabs/Result/TargetCapsule"));
        bag.target = target;

        yield return new WaitForSeconds(15f);

        // TearDown
        Object.Destroy(target.gameObject);
        bag.Destroy();
    }

}
