using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UnityEngineSpecTest
{
    private ResourceLoader resourceLoader;
    private GameInfo gameInfo;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        gameInfo = Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(resourceLoader.gameObject);
        Object.Destroy(gameInfo.gameObject);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Destroy([componentReference]) destroys only attached component. </ br>
    /// </summary>
    public IEnumerator ComponentDestroyTest()
    {
        var prefabDoorV = Resources.Load<DoorControl>("Prefabs/Map/DoorV");
        var prefabDoorH = Resources.Load<DoorHControl>("Prefabs/Map/DoorH");

        yield return null;

        var matSource = ResourceLoader.Instance.floorMaterialsData.Param(0);

        yield return null;

        var doorV = Object.Instantiate(prefabDoorV, new Vector3(-2.5f, 0, 0), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        doorV.SetState(new DoorState());
        var doorV2 = Object.Instantiate(prefabDoorV, new Vector3(-2.5f, 0, 2.5f), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        doorV2.SetState(new DoorState());
        var doorH = Object.Instantiate(prefabDoorH, new Vector3(0, 0, 0), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        doorH.SetState(new DoorState());
        var doorH2 = Object.Instantiate(prefabDoorH, new Vector3(0, 0, 2.5f), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        doorH2.SetState(new DoorState());

        var doorVTf = doorV.transform;
        var doorV2Tf = doorV2.transform;
        var doorHTf = doorH.transform;
        var doorH2Tf = doorH2.transform;

        yield return new WaitForSeconds(1f);

        Object.Destroy(doorV);
        GameObject.Destroy(doorV2);
        MonoBehaviour.Destroy(doorH);
        DoorHControl.Destroy(doorH2);

        yield return new WaitForSeconds(1f);

        Assert.False(doorVTf == null);
        Assert.False(doorV2Tf == null);
        Assert.False(doorHTf == null);
        Assert.False(doorH2Tf == null);

        yield return new WaitForSeconds(1f);

        // TearDown
        Object.Destroy(doorVTf.gameObject);
        GameObject.Destroy(doorV2Tf.gameObject);
        MonoBehaviour.Destroy(doorHTf.gameObject);
        DoorHControl.Destroy(doorH2Tf.gameObject);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Any Object inherited class Destroy can destroy GameObject.
    /// </summary>
    public IEnumerator GameObjectDestroyTest()
    {
        var prefabDoorV = Resources.Load<DoorControl>("Prefabs/Map/DoorV");
        var prefabDoorH = Resources.Load<DoorHControl>("Prefabs/Map/DoorH");

        yield return null;

        var matSource = ResourceLoader.Instance.floorMaterialsData.Param(0);

        yield return null;

        var doorV = Object.Instantiate(prefabDoorV, new Vector3(-2.5f, 0, 0), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        doorV.SetState(new DoorState());
        var doorV2 = Object.Instantiate(prefabDoorV, new Vector3(-2.5f, 0, 2.5f), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        doorV2.SetState(new DoorState());
        var doorH = Object.Instantiate(prefabDoorH, new Vector3(0, 0, 0), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        doorH.SetState(new DoorState());
        var doorH2 = Object.Instantiate(prefabDoorH, new Vector3(0, 0, 2.5f), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        doorH2.SetState(new DoorState());

        var doorVTf = doorV.transform;
        var doorV2Tf = doorV2.transform;
        var doorHTf = doorH.transform;
        var doorH2Tf = doorH2.transform;

        yield return new WaitForSeconds(1f);

        Object.Destroy(doorV.gameObject);
        GameObject.Destroy(doorV2.gameObject);
        MonoBehaviour.Destroy(doorH.gameObject);
        DoorHControl.Destroy(doorH2.gameObject);

        yield return new WaitForSeconds(1f);

        Assert.True(doorVTf == null);
        Assert.True(doorV2Tf == null);
        Assert.True(doorHTf == null);
        Assert.True(doorH2Tf == null);

        yield return new WaitForSeconds(1f);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Destroyed GameObject cannot be compared with interface type. </ br>
    /// Interface variable contains {null} GameObject after Destroy() the GameObject. </ br>
    /// </summary>
    public IEnumerator NullComparisonTestWithDestroyedGameObject()
    {
        TestInterface component = new GameObject("TestComponent").AddComponent(typeof(TestComponent)) as TestInterface;
        yield return null;

        Assert.IsNotNull(component);
        Assert.IsNotNull(component.gameObject);

        yield return null;

        Object.Destroy(component.gameObject);

        yield return null;

        // component contains {null} GameObject
        Assert.IsNotNull(component);
        Assert.False(component == null);

        // component cannot access destroyed gameObject
        Assert.That(
            () => component.gameObject,
            Throws.TypeOf<MissingReferenceException>()
        );

        // MonoBehaviour can be compared with destroyed({null}) GameObject
        Assert.True(component as MonoBehaviour == null);
    }

    public interface TestInterface
    {
        GameObject gameObject { get; }
    }
    public class TestComponent : MonoBehaviour, TestInterface { }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    public IEnumerator AnimatorParamChangeIsAppliedOnTheSameFrame()
    {

        var slime = Object.Instantiate(Resources.Load<GameObject>("Prefabs/TestSlimeGreen"));
        var anim = slime.GetComponent<Animator>();
        var hashedParam = Animator.StringToHash("Die");
        anim.SetBool(hashedParam, false);

        yield return new WaitForSeconds(1f);

        anim.SetBool(hashedParam, true);

        Assert.True(anim.GetBool(hashedParam));

        yield return null;

        Object.Destroy(slime);
    }
}
