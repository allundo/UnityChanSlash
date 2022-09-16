using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UniRx;
using System.Text;
using DG.Tweening;

public class UnityEngineSpecTest
{
    private ResourceLoader resourceLoader;
    private GameInfo gameInfo;
    private Camera prefabCamera;
    private GameObject prefabSlime;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        gameInfo = Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));
        prefabCamera = Resources.Load<Camera>("Prefabs/TestCamera");
        prefabSlime = Resources.Load<GameObject>("Prefabs/TestSlimeGreen");
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
    public IEnumerator _001_ComponentDestroyTest()
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
    public IEnumerator _002_GameObjectDestroyTest()
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
    public IEnumerator _003_NullComparisonTestWithDestroyedGameObject()
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
    public IEnumerator _004_AnimatorParamChangeIsAppliedOnTheSameFrame()
    {
        var mainCamera = Object.Instantiate(prefabCamera);

        var slime = Object.Instantiate(prefabSlime);
        var anim = slime.GetComponent<Animator>();
        var hashedParam = Animator.StringToHash("Die");
        anim.SetBool(hashedParam, false);

        yield return new WaitForSeconds(1f);

        anim.SetBool(hashedParam, true);

        Assert.True(anim.GetBool(hashedParam));

        yield return null;

        Object.Destroy(slime);
        Object.Destroy(mainCamera);
    }

    public class NonSerializableClass
    {
        public int num = 64;
        public string text = "default";

        public byte[] byteArray = new byte[] { 0x23, 0xF4, 0x45, 0x33 };
    }

    [System.Serializable]
    public class SerializableClass
    {
        public int num = 64;
        public string text = "default";

        public byte[] byteArray = new byte[] { 0x23, 0xF4, 0x45, 0x33 };
    }

    [System.Serializable]
    public class NestedSerializableClass
    {
        public SerializableClass[] serializableClasses = new object[2].Select(_ => new SerializableClass()).ToArray();
        public SerializableClass singleClass = new SerializableClass();
    }

    public List<int> intList = new List<int>() { 1, 2, 3, 4 };

    public List<SerializableClass> serializableClassList = new object[3].Select(_ => new SerializableClass()).ToList();
    public List<NonSerializableClass> nonSerializableClassList = new object[3].Select(_ => new NonSerializableClass()).ToList();

    [Ignore("Only for spec confirmation.")]
    [Test]
    /// <summary>
    /// Nested serializable class can be convert to JSON. <br />
    /// Arrays can be convert to JSON only inside serializable class.
    /// </summary>
    public void _005_UnityJsonSerializeTest()
    {
        Assert.AreEqual("{\"num\":64,\"text\":\"default\",\"byteArray\":[35,244,69,51]}", JsonUtility.ToJson(new SerializableClass()));
        Assert.AreEqual("{\"num\":64,\"text\":\"default\",\"byteArray\":[35,244,69,51]}", JsonUtility.ToJson(new NonSerializableClass()));
        Assert.AreEqual("{}", JsonUtility.ToJson(intList));
        Assert.AreEqual("{}", JsonUtility.ToJson(intList.ToArray()));
        Assert.AreEqual("{}", JsonUtility.ToJson(serializableClassList));
        Assert.AreEqual("{}", JsonUtility.ToJson(serializableClassList.ToArray()));
        Assert.AreEqual("{}", JsonUtility.ToJson(nonSerializableClassList));
        Assert.AreEqual("{}", JsonUtility.ToJson(nonSerializableClassList.ToArray()));

        Assert.AreEqual(
            "{\"serializableClasses\":"
                + "[{\"num\":64,\"text\":\"default\",\"byteArray\":[35,244,69,51]},{\"num\":64,\"text\":\"default\",\"byteArray\":[35,244,69,51]}],"
                + "\"singleClass\":{\"num\":64,\"text\":\"default\",\"byteArray\":[35,244,69,51]}}",
                JsonUtility.ToJson(new NestedSerializableClass())
        );
    }

    [Ignore("Only for spec confirmation.")]
    [Test]
    /// <summary>
    /// ContinueWith receives OnCompleted and passes the last message only.<br />
    /// SelectMany receives OnNext and passes it for each time.
    /// </summary>
    public void _006_DifferenceBetweenContinueWithAndSelectManyTest()
    {
        int count1, count2, count3, count4;
        ISubject<string> subject = new Subject<string>();
        ISubject<string> completeOnly = new Subject<string>();

        StringBuilder sbContinueWith = new StringBuilder("ContinueWith: ");
        StringBuilder sbSelectMany = new StringBuilder("SelectMany: ");

        StringBuilder sbContinueWithComp = new StringBuilder("ContinueWithComp: ");
        StringBuilder sbSelectManyComp = new StringBuilder("SelectManyComp: ");

        count1 = count2 = count3 = count4 = 0;

        subject
            .ContinueWith(str => Observable.Return(count1++ + ":" + str + ", "))
            .Subscribe(str => sbContinueWith.Append(str));

        subject
            .SelectMany(str => Observable.Return(count2++ + ":" + str + ", "))
            .Subscribe(str => sbSelectMany.Append(str));

        completeOnly
            .ContinueWith(str => Observable.Return(count3++ + ":" + str + ", "))
            .Subscribe(str => sbContinueWithComp.Append(str));

        completeOnly
            .SelectMany(str => Observable.Return(count4++ + ":" + str + ", "))
            .Subscribe(str => sbSelectManyComp.Append(str));

        subject.OnNext("aaaa");
        subject.OnNext("bbbb");
        subject.OnNext("cccc");

        Assert.AreEqual("ContinueWith: ", sbContinueWith.ToString());
        Assert.AreEqual("SelectMany: 0:aaaa, 1:bbbb, 2:cccc, ", sbSelectMany.ToString());

        subject.OnCompleted();
        completeOnly.OnCompleted();

        Assert.AreEqual("ContinueWith: 0:cccc, ", sbContinueWith.ToString());
        Assert.AreEqual("SelectMany: 0:aaaa, 1:bbbb, 2:cccc, ", sbSelectMany.ToString());

        Assert.AreEqual("ContinueWithComp: ", sbContinueWithComp.ToString());
        Assert.AreEqual("SelectManyComp: ", sbSelectManyComp.ToString());
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Different axis tween moves can be applied at the same time. <br />
    /// </summary>
    public IEnumerator _007_TweenMoveWithSeparatedAxis()
    {
        var mainCamera = Object.Instantiate(prefabCamera);
        var slime = Object.Instantiate(prefabSlime, new Vector3(-0.25f, 0f, 0f), Quaternion.identity);

        var tf = slime.transform;

        yield return null;

        var seq = DOTween.Sequence()
            .Join(tf.DOMoveX(0.5f, 2f).SetRelative().SetEase(Ease.Linear))
            .Join(tf.DOMoveY(0.5f, 2f).SetRelative().SetEase(Ease.OutQuad))
            .Play();

        yield return new WaitForSeconds(1f);

        Assert.AreEqual(0f, tf.position.x, 0.001f);
        Assert.AreEqual(0.375f, tf.position.y, 0.001f);


        yield return new WaitForSeconds(1f);

        Assert.AreEqual(0.25f, tf.position.x, 0.001f);
        Assert.AreEqual(0.5f, tf.position.y, 0.001f);

        Object.Destroy(slime);
        Object.Destroy(mainCamera.gameObject);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    public IEnumerator _008_AxisBaseTweenMoveAlsoReceivesDestination()
    {
        var mainCamera = Object.Instantiate(prefabCamera);
        var slime = Object.Instantiate(prefabSlime, new Vector3(-0.25f, 0f, 0f), Quaternion.identity);

        var tf = slime.transform;

        yield return null;

        var seq = DOTween.Sequence()
            .Join(tf.DOMoveX(0.5f, 2f).SetEase(Ease.Linear))
            .Join(tf.DOMoveY(0.5f, 2f).SetEase(Ease.OutQuad))
            .Play();

        yield return new WaitForSeconds(1f);

        Assert.AreEqual(0.125f, tf.position.x, 0.001f);
        Assert.AreEqual(0.375f, tf.position.y, 0.001f);

        yield return new WaitForSeconds(1f);

        Assert.AreEqual(0.5f, tf.position.x, 0.001f);
        Assert.AreEqual(0.5f, tf.position.y, 0.001f);

        Object.Destroy(slime);
        Object.Destroy(mainCamera.gameObject);
    }
}
