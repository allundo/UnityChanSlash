using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ResultTest
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var camera = Object.Instantiate(Resources.Load<Camera>("Prefabs/Result/ResultCamera"));
        Object.Instantiate(Resources.Load<GameObject>("Prefabs/Result/Ground"));

        var defaultPos = camera.transform.position;
        camera.transform.position = new Vector3(0, defaultPos.y, defaultPos.z);
    }

    [SetUp]
    public void SetUp()
    {
    }

    [TearDown]
    public void TearDown()
    {
    }

    [UnityTest]
    public IEnumerator _001_MoneyBagDropTest()
    {
        var bag = Object.Instantiate(Resources.Load<BagControl>("Prefabs/Result/BagControls"), new Vector3(0, 5f, 0f), Quaternion.identity);
        var target = Object.Instantiate(Resources.Load<CapsuleCollider>("Prefabs/Result/TargetCapsule"));
        bag.target = target;

        yield return new WaitForSeconds(15f);
    }

}
