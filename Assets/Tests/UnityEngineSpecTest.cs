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
    private TestMonoBehaviour prefabTestGameObject;
    private TestMonoBehaviour prefabTestInactiveMonoBehaviour;
    private TestNestedMonoBehaviour prefabTestNestedMonoBehaviour;
    private TestMonoBehaviourTree prefabTestMonoBehaviourTree;
    private TestUniRxTree prefabTestUniRxTree;
    private GameObject prefabLock;
    private Canvas prefabCanvas;
    private TestDrag prefabDrag;
    private AnimatorTest prefabAnna;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        gameInfo = Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));
        prefabCamera = Resources.Load<Camera>("Prefabs/TestCamera");
        prefabSlime = Resources.Load<GameObject>("Prefabs/TestSlimeGreen");
        prefabTestGameObject = Resources.Load<TestMonoBehaviour>("Prefabs/TestGameObject");
        prefabTestInactiveMonoBehaviour = Resources.Load<TestMonoBehaviour>("Prefabs/TestInactiveMonoBehaviour");
        prefabTestNestedMonoBehaviour = Resources.Load<TestNestedMonoBehaviour>("Prefabs/TestNestedMonoBehaviour");
        prefabTestMonoBehaviourTree = Resources.Load<TestMonoBehaviourTree>("Prefabs/TestMonoBehaviourTree");
        prefabTestUniRxTree = Resources.Load<TestUniRxTree>("Prefabs/TestUniRxTree");
        prefabLock = Resources.Load<GameObject>("Prefabs/TestLock");
        prefabCanvas = Resources.Load<Canvas>("Prefabs/UI/Canvas");
        prefabDrag = Resources.Load<TestDrag>("Prefabs/UI/TestDrag");
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
        var doorV2 = Object.Instantiate(prefabDoorV, new Vector3(-2.5f, 0, 2.5f), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        var doorH = Object.Instantiate(prefabDoorH, new Vector3(0, 0, 0), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        var doorH2 = Object.Instantiate(prefabDoorH, new Vector3(0, 0, 2.5f), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);

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
        var doorV2 = Object.Instantiate(prefabDoorV, new Vector3(-2.5f, 0, 2.5f), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        var doorH = Object.Instantiate(prefabDoorH, new Vector3(0, 0, 0), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);
        var doorH2 = Object.Instantiate(prefabDoorH, new Vector3(0, 0, 2.5f), Quaternion.identity).SetMaterials(matSource.gate, matSource.door);

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

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    public IEnumerator _009_CompareTweens()
    {
        var list = new List<Tween>();
        var tween = DOVirtual.DelayedCall(2f, () => Debug.Log("Test Tween"));

        list.Add(tween);

        Assert.AreEqual(tween, tween.OnComplete(() => Debug.Log("OnComplete")));

        Assert.AreEqual(1, list.Count);

        list.Remove(tween.Play());

        Assert.AreEqual(0, list.Count);

        yield return new WaitForSeconds(2f);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    public IEnumerator _010_OnPlayAndOnCompleteTimingTest()
    {
        bool[] flags = new bool[10];

        DOTween.Sequence()
            .AppendCallback(() =>
            {
                flags[0] = true;
            })
            .AppendCallback(() =>
            {
                flags[1] = true;
            })
            .Join(DOVirtual.DelayedCall(0.5f, () =>
            {
                flags[2] = true;
            })
            .OnPlay(() =>
            {
                flags[3] = true;
            })
            .OnComplete(() =>
            {
                flags[4] = true;
            }))
            .Join(
                DOTween.Sequence()
                    .AppendCallback(() => flags[8] = true)
                    .Append(DOVirtual.DelayedCall(0.5f, () => flags[9] = true))
            )
            .AppendCallback(() =>
            {
                flags[5] = true;
            })
            .OnPlay(() =>
            {
                flags[6] = true;
            })
            .OnComplete(() =>
            {
                flags[7] = true;
            })
            .Play();

        // No flags are active on Play() called frame
        Assert.False(flags[0]);
        Assert.False(flags[1]);
        Assert.False(flags[2]);
        Assert.False(flags[3]);
        Assert.False(flags[4]);
        Assert.False(flags[5]);
        Assert.False(flags[6]);
        Assert.False(flags[7]);

        yield return new WaitForEndOfFrame();

        // Even OnPlay() or first AppendCallback, the flags are still inactive on the next frame of Play() called

        Assert.False(flags[0]);
        Assert.False(flags[1]);
        Assert.False(flags[2]);
        Assert.False(flags[3]);
        Assert.False(flags[4]);
        Assert.False(flags[5]);
        Assert.False(flags[6]);
        Assert.False(flags[7]);
        Assert.False(flags[8]);
        Assert.False(flags[9]);

        yield return new WaitForEndOfFrame();

        Assert.True(flags[0]);      // The first AppendCallback is activated
        Assert.True(flags[1]);      // The second AppendCallback is activated
        Assert.False(flags[2]);
        Assert.True(flags[3]);      // OnPlay() of joined tween is activated
        Assert.False(flags[4]);
        Assert.False(flags[5]);
        Assert.True(flags[6]);      // OnPlay() of the sequence is activated
        Assert.False(flags[7]);
        Assert.True(flags[8]);      // The first AppendCallback of the nested sequence is activated
        Assert.False(flags[9]);

        yield return new WaitForEndOfFrame();

        Assert.True(flags[0]);
        Assert.True(flags[1]);
        Assert.False(flags[2]);
        Assert.True(flags[3]);
        Assert.False(flags[4]);
        Assert.False(flags[5]);
        Assert.True(flags[6]);
        Assert.False(flags[7]);
        Assert.True(flags[8]);
        Assert.False(flags[9]);

        yield return new WaitForSeconds(0.5f);

        Assert.True(flags[0]);
        Assert.True(flags[1]);
        Assert.True(flags[2]);      // Appended DelayedCall is activated
        Assert.True(flags[3]);
        Assert.True(flags[4]);      // OnComplete() of joined tween is activated
        Assert.True(flags[5]);      // The last AppendCallback is activated
        Assert.True(flags[6]);
        Assert.True(flags[7]);      // OnComplete() of the sequence is activated
        Assert.True(flags[8]);
        Assert.True(flags[9]);      // DelayedCall of the nested sequence is activated
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// ContinueWith() cannot continue the stream with the OnNext() that is fired before the observable is completed.
    /// </summary>
    public IEnumerator _011_ContinueWithTest()
    {
        var sb1 = new StringBuilder();
        var sb2 = new StringBuilder();
        var observable1 = DOVirtual.DelayedCall(0.5f, () => Debug.Log("observable1, ")).OnCompleteAsObservable(Unit.Default).Publish();
        var observable2 = DOVirtual.DelayedCall(1f, () => Debug.Log("observable2, ")).OnCompleteAsObservable(Unit.Default);

        var subject = new Subject<Unit>();

        var observable4 = Observable.Merge(observable1, observable2);

        observable4.ContinueWith(_ =>           // Continue after 1 sec.
            {
                sb1.Append("ContinueWith1");
                Debug.Log("ContinueWith1, ");
                return subject;                 // OnNext at 0.75 sec.
            })
            .ContinueWith(_ =>                  // Cannot continue
            {
                sb2.Append("ContinueWith2");
                Debug.Log("ContinueWith2, ");
                return DOVirtual.DelayedCall(0.5f, () => Debug.Log("observable4")).OnCompleteAsObservable(Unit.Default);
            })
            .Subscribe(_ => sb1.Append("Observed"));

        observable1.ContinueWith(_ =>           // Continue after 0.5 sec.
            {
                sb2.Append("ContinueWith1");
                return subject;                 // OnNext at 0.75 sec.
            })
            .ContinueWith(_ =>                  // Continues the stream OnCompleted at 1.75 sec.
            {
                sb2.Append("ContinueWith2");
                return DOVirtual.DelayedCall(0.5f, () => Debug.Log("observable4")).OnCompleteAsObservable(Unit.Default);
            })
            .Subscribe(_ => sb2.Append("Observed"));

        observable1.Connect();

        yield return new WaitForSeconds(0.75f);

        Debug.Log("OnNext, ");
        subject.OnNext(Unit.Default);

        yield return new WaitForSeconds(1f);

        Debug.Log("OnCompleted, ");
        subject.OnCompleted();

        Assert.AreEqual("ContinueWith1", sb1.ToString());
        Assert.AreEqual("ContinueWith1ContinueWith2", sb2.ToString());

        yield return new WaitForSeconds(0.75f);

        Assert.AreEqual("ContinueWith1", sb1.ToString());
        Assert.AreEqual("ContinueWith1ContinueWith2Observed", sb2.ToString());
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    public IEnumerator _012_AwakeIsCalledImmediatelyAfterInstantiated()
    {
        var mainCamera = Object.Instantiate(prefabCamera);

        yield return null;

        var sut = Object.Instantiate(prefabTestGameObject, new Vector3(-0.25f, 0f, 0f), Quaternion.identity);
        var sutNested = Object.Instantiate(prefabTestNestedMonoBehaviour, new Vector3(-0.25f, 0f, 0f), Quaternion.identity);
        var sutTree = Object.Instantiate(prefabTestMonoBehaviourTree, new Vector3(-0.25f, 0f, 0f), Quaternion.identity);
        var sutUniRxTree = Object.Instantiate(prefabTestUniRxTree, new Vector3(-0.25f, 0f, 0f), Quaternion.identity);

        Assert.IsTrue(sut.isAwakeCalled);
        Assert.IsFalse(sut.isStartCalled);
        Assert.IsFalse(sut.isUpdateCalled);

        Assert.IsFalse(sutNested.isChildAwakeCalled);
        Assert.IsFalse(sutNested.isChildStartCalled);
        Assert.IsFalse(sutNested.isChildUpdateCalled);

        Assert.IsTrue(sutTree.isChildAwakeCalled);
        Assert.IsFalse(sutTree.isChildStartCalled);
        Assert.IsFalse(sutTree.isChildUpdateCalled);

        yield return null;

        Assert.IsTrue(sut.isAwakeCalled);
        Assert.IsTrue(sut.isStartCalled);
        Assert.IsTrue(sut.isUpdateCalled);

        Assert.IsTrue(sutNested.isChildAwakeCalled);
        Assert.IsTrue(sutNested.isChildStartCalled);
        Assert.IsTrue(sutNested.isChildUpdateCalled);

        Assert.IsTrue(sutTree.isChildAwakeCalled);
        Assert.IsTrue(sutTree.isChildStartCalled);
        Assert.IsTrue(sutTree.isChildUpdateCalled);

        yield return new WaitForSeconds(1f);

        Object.Destroy(sut.gameObject);
        Object.Destroy(sutNested.gameObject);
        Object.Destroy(sutTree.gameObject);
        Object.Destroy(sutUniRxTree.gameObject);
        Object.Destroy(mainCamera.gameObject);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    public IEnumerator _013_AwakeIsNotCalledIfNotActive()
    {
        var mainCamera = Object.Instantiate(prefabCamera);

        yield return null;

        var sut = Object.Instantiate(prefabTestInactiveMonoBehaviour, new Vector3(-0.25f, 0f, 0f), Quaternion.identity);
        var sutChild = Object.Instantiate(prefabTestGameObject, new Vector3(-0.25f, 0f, 0f), Quaternion.identity, sut.transform);

        Assert.IsFalse(sut.isAwakeCalled);          // The inactivated object doesn't call Awake() on instantiated timing.
        Assert.IsFalse(sut.isStartCalled);
        Assert.IsFalse(sut.isUpdateCalled);

        Assert.IsFalse(sutChild.isAwakeCalled);     // The child object that is instantiated under an inactivate object also doesn't call Awake().
        Assert.IsFalse(sutChild.isStartCalled);
        Assert.IsFalse(sutChild.isUpdateCalled);

        yield return null;

        Assert.IsFalse(sut.isAwakeCalled);
        Assert.IsFalse(sut.isStartCalled);
        Assert.IsFalse(sut.isUpdateCalled);

        Assert.IsFalse(sutChild.isAwakeCalled);
        Assert.IsFalse(sutChild.isStartCalled);
        Assert.IsFalse(sutChild.isUpdateCalled);

        sut.gameObject.SetActive(true);

        Assert.IsTrue(sut.isAwakeCalled);           // Only Awake() is called on activated timing.
        Assert.IsFalse(sut.isStartCalled);
        Assert.IsFalse(sut.isUpdateCalled);

        Assert.IsTrue(sutChild.isAwakeCalled);      // The child object is the same.
        Assert.IsFalse(sutChild.isStartCalled);
        Assert.IsFalse(sutChild.isUpdateCalled);

        yield return null;

        Assert.IsTrue(sut.isAwakeCalled);
        Assert.IsTrue(sut.isStartCalled);
        Assert.IsTrue(sut.isUpdateCalled);

        Assert.IsTrue(sutChild.isAwakeCalled);
        Assert.IsTrue(sutChild.isStartCalled);
        Assert.IsTrue(sutChild.isUpdateCalled);

        yield return new WaitForSeconds(1f);

        Object.Destroy(sut.gameObject);
        Object.Destroy(sutChild.gameObject);
        Object.Destroy(mainCamera.gameObject);
    }

    [UnityTest]
    /// <summary>
    /// Interruption source option "Next state" enables skipping transitions any times but one by one.
    /// </summary>
    public IEnumerator _014_AnimatorTransitionTest()
    {
        var mainCamera = Object.Instantiate(prefabCamera);
        var lockAnim = Object.Instantiate(prefabLock, new Vector3(0f, 0f, -5f), Quaternion.identity).GetComponent<Animator>();

        int[] triggerValues = new string[] { "Unlock00", "Lock00", "Unlock01", "Lock01" }
            .Select(name => Animator.StringToHash(name)).ToArray();

        foreach (var value in triggerValues)
        {
            lockAnim.SetBool("Enable", true);
            lockAnim.SetTrigger(value);
            yield return new WaitForSeconds(2f);
            lockAnim.SetBool("Enable", false);
        }

        foreach (var value in triggerValues)
        {
            lockAnim.SetBool("Enable", true);
            lockAnim.SetTrigger(value);
            yield return new WaitForSeconds(1f);
            lockAnim.SetBool("Enable", false);
        }

        foreach (var value in triggerValues)
        {
            lockAnim.SetBool("Enable", true);
            lockAnim.SetTrigger(value);
            yield return new WaitForSeconds(0.5f);
            lockAnim.SetBool("Enable", false);
        }

        foreach (var value in triggerValues)
        {
            lockAnim.SetBool("Enable", true);
            lockAnim.SetTrigger(value);
            yield return new WaitForSeconds(0.25f);
            lockAnim.SetBool("Enable", false);
        }

        yield return new WaitForSeconds(1f);

        Object.Destroy(lockAnim.gameObject);
        Object.Destroy(mainCamera.gameObject);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Tween callback maybe called by "yield WaitForSeconds". <br />
    /// Execution order is not deterministic.
    /// </summary>
    public IEnumerator _015_TweenOnCompleteTimingTest()
    {
        yield return null;

        var sb = new StringBuilder();
        var updateTime = new GameObject("UpdateTime").AddComponent<UpdateTime>();
        updateTime.SetStringBuilder(sb);

        DOVirtual.DelayedCall(1f, () => sb.Append("DelayedCall(onComplete) 1sec, ")).Play();

        yield return new WaitForSeconds(1f);

        sb.Append("WaitForSeconds 1sec, ");

        // !CAUTION! -> WaitForSeconds can be executed before DelayedCall.
        Assert.AreEqual("DelayedCall(onComplete) 1sec, WaitForSeconds 1sec, ", sb.ToString());
        yield return null;

        Assert.AreEqual("DelayedCall(onComplete) 1sec, WaitForSeconds 1sec, Update() 1sec, ", sb.ToString());
    }

    public class UpdateTime : MonoBehaviour
    {
        private float elapsed = 0f;
        private StringBuilder sb;

        private const float FRAME_SEC_UNIT = 0.01666666667f;

        void Start()
        {
            elapsed -= FRAME_SEC_UNIT;
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed > 1f)
            {
                sb.Append($"Update() 1sec, ");
                elapsed -= 1f;
                Debug.Log($"Overrun: {elapsed}sec");
            }
        }

        public void SetStringBuilder(StringBuilder sb)
        {
            this.sb = sb;
        }
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Coroutine restarts after Update() anyway. <br />
    /// Update() stops at the end of frame that Destroy() is called.
    /// </summary>
    public IEnumerator _016_DestroyAndCoroutineTimingTest()
    {
        var destroyTest = new GameObject("DestroyTest").AddComponent<DestroyTestObject>();
        var counter = new TestCounter();
        destroyTest.SetCounter(counter);

        Assert.AreEqual(0, counter.Count);
        yield return new WaitForEndOfFrame();

        // V----- End of frame 0 -----V
        Assert.AreEqual(0, counter.Count);
        yield return new WaitForEndOfFrame();

        // V----- End of frame 1 -----V
        Assert.AreEqual(1, counter.Count);
        destroyTest.Destroy();
        yield return new WaitForSeconds(2f);

        // DestroyTestObject.Update() in frame 2 was not applied.
        Assert.AreEqual(1, counter.Count);
        yield return null;

        // V----- After Update() in frame -1 -----V
        var destroyTest2 = new GameObject("DestroyTest").AddComponent<DestroyTestObject>();
        var counter2 = new TestCounter();
        destroyTest2.SetCounter(counter2);

        Assert.AreEqual(0, counter2.Count);
        yield return null;

        // V----- After Update() in frame 0 -----V
        Assert.AreEqual(1, counter2.Count);
        yield return null;

        // V----- After Update() in frame 1 -----V
        Assert.AreEqual(2, counter2.Count);
        destroyTest2.Destroy();
        yield return new WaitForSeconds(2f);

        // DestroyTestObject.Update() on frame 2 was not applied.
        Assert.AreEqual(2, counter2.Count);
    }

    public class DestroyTestObject : MonoBehaviour
    {
        private TestCounter counter;
        public void SetCounter(TestCounter counter) => this.counter = counter;

        void Update() { counter.Inc(); }
        public void Destroy() => Destroy(gameObject);
    }

    public class TestCounter
    {
        private int count = 0;
        public int Count => count;
        public void Inc() => count++;
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    public IEnumerator _017_TweenFullPositionMeansElapsedSecondFromPlay()
    {
        float sec1, sec2, sec3, sec4;
        sec1 = sec2 = sec3 = sec4 = 0.0f;

        var sut = DOVirtual.DelayedCall(5f, () => { }).Play();
        var seq = DOTween.Sequence()
            .AppendInterval(1f)
            .AppendCallback(() => sec1 = sut.fullPosition)
            .AppendInterval(1f)
            .AppendCallback(() => sec2 = sut.fullPosition)
            .AppendInterval(1f)
            .AppendCallback(() => sec3 = sut.fullPosition)
            .AppendInterval(1f)
            .AppendCallback(() => sec4 = sut.fullPosition)
            .Play();

        yield return new WaitForSeconds(5f);

        Assert.AreEqual(1f, sec1, Constants.FRAME_SEC_UNIT);
        Assert.AreEqual(2f, sec2, Constants.FRAME_SEC_UNIT);
        Assert.AreEqual(3f, sec3, Constants.FRAME_SEC_UNIT);
        Assert.AreEqual(4f, sec4, Constants.FRAME_SEC_UNIT);

        yield return null;
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    public IEnumerator _018_OnDragDoesntWorkAfterDestroy()
    {
        var eventSystem = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/EventSystem"));
        var canvas = Object.Instantiate(prefabCanvas);
        var drag = Object.Instantiate(prefabDrag, canvas.transform);

        yield return new WaitForSeconds(5f);

        Object.Destroy(drag);

        yield return new WaitForSeconds(5f);
        Object.Destroy(canvas);
        Object.Destroy(eventSystem);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    public IEnumerator _019_ColliderDoesntDetectsWhenDisabledOnAwake()
    {
        var cube = Object.Instantiate(Resources.Load<DetectSphere>("Prefabs/CubeDetector"));
        var sphere = Object.Instantiate(Resources.Load<TestSphere>("Prefabs/TestSphere"));

        yield return null;

        Assert.False(sphere.isDetected);

        yield return null;

        Object.Destroy(cube.gameObject);
        Object.Destroy(sphere.gameObject);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Animator params are applied on the same frame as the frame that those params are set. <br />
    /// </summary>
    public IEnumerator _020_AnimatorSetBoolAppliesValueImmediately()
    {
        var mainCamera = Object.Instantiate(prefabCamera);
        var slime = Object.Instantiate(prefabSlime);

        var anim = slime.GetComponent<Animator>();

        anim.SetBool("Die", false);
        bool param1 = anim.GetBool("Die");

        yield return null;

        anim.SetBool("Die", true);
        bool param2 = anim.GetBool("Die");

        yield return null;

        bool param3 = anim.GetBool("Die");

        yield return null;

        bool param4 = anim.GetBool("Die");

        yield return null;

        anim.SetBool("Die", false);
        bool param5 = anim.GetBool("Die");

        yield return null;

        anim.SetBool("Die", true);
        bool param6 = anim.GetBool("Die");

        yield return null;

        bool param7 = anim.GetBool("Die");

        yield return new WaitForSeconds(1f);

        bool param8 = anim.GetBool("Die");

        Assert.AreEqual(false, param1);
        Assert.AreEqual(true, param2);
        Assert.AreEqual(true, param3);
        Assert.AreEqual(true, param4);
        Assert.AreEqual(false, param5);
        Assert.AreEqual(true, param6);
        Assert.AreEqual(true, param7);
        Assert.AreEqual(true, param8);

        Object.Destroy(slime);
        Object.Destroy(mainCamera.gameObject);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Transitions from 2DFreeformDirectional start immediately after transit condition is satisfied <br />
    /// without control parameters become zero.
    /// </summary>
    public IEnumerator _021_CanTransitFrom2DFreeformWithParameterBool()
    {
        var mainCamera = Object.Instantiate(prefabCamera);
        var annaAnim = Object.Instantiate(Resources.Load<AnimatorTest>("Prefabs/Character/Anna2DFreeFormTest"), new Vector3(0f, 0f, -5f), Quaternion.identity);
        yield return new WaitForSeconds(1f);

        annaAnim.StartMoving(4f);

        yield return new WaitForSeconds(1f);

        // Current state is Move
        Assert.True(annaAnim.IsCurrentState("Move"));

        // Set the transition flag.
        annaAnim.EndMoving();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // Transition Duration is 0.05 so this transition will progress over half.
        yield return new WaitForSeconds(0.03f);

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        // Transition Duration is 0.05 so this transition will have finished.
        yield return new WaitForSeconds(0.02f);

        Assert.False(annaAnim.IsCurrentState("Move"));
        Assert.True(annaAnim.IsCurrentState("Idle"));

        annaAnim.StartMoving(4f);

        yield return new WaitForSeconds(1f);

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        // Set the transition flag.
        annaAnim.EndMoving();
        annaAnim.attack.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // Transition Duration is 0.05 so this transition will progress over half.
        yield return new WaitForSeconds(0.03f);

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));
        Assert.False(annaAnim.IsCurrentState("Attack"));

        // Transition Duration is 0.05 but the transition was switched to Attack.
        yield return new WaitForSeconds(0.02f);

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));
        Assert.False(annaAnim.IsCurrentState("Attack"));

        // Transition Duration to Attack is 0.1 so this transition will have finished.
        yield return new WaitForSeconds(0.05f);
        Assert.False(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));
        Assert.True(annaAnim.IsCurrentState("Attack"));

        yield return new WaitForSeconds(1f);

        Object.Destroy(annaAnim.gameObject);
        Object.Destroy(mainCamera);
        yield return null;
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Transition with "Current State" interruption source switches back to transition to "Exit" state.
    /// </summary>
    public IEnumerator _022_InterruptionSourceCurrentStateSwitchesBackToTransitionToExitStateAgain()
    {
        var mainCamera = Object.Instantiate(prefabCamera);
        var annaAnim = Object.Instantiate(Resources.Load<AnimatorTest>("Prefabs/Character/Anna2DFreeFormTest"), new Vector3(0f, 0f, -5f), Quaternion.identity);
        yield return new WaitForSeconds(1f);

        annaAnim.StartMoving(4f);

        yield return new WaitForSeconds(1f);

        // Current state is Move
        Assert.True(annaAnim.IsCurrentState("Move"));

        // Start "Next State" transition with interruption source "Current State".
        annaAnim.EndMoving();
        annaAnim.interruption.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // Transition Move -> Exit starts.
        Assert.True(annaAnim.IsCurrentTransition("Move", "Exit"));

        yield return null;

        // The transition switched to Idle -> Interruption.
        Assert.True(annaAnim.IsCurrentTransition("Idle", "Interruption"));

        yield return null;

        // The transition switched back to Move -> Exit. This behavior may be bug.
        Assert.True(annaAnim.IsCurrentTransition("Move", "Exit"));

        yield return null;

        // Transition Duration is 0.05 so this transition should have finished but not.
        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Interruption"));

        // Still moving at the next frame.
        yield return null;

        Assert.True(annaAnim.IsCurrentState("Move"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        // Moving finishes at the 2nd frame.
        yield return null;

        Assert.False(annaAnim.IsCurrentState("Move"));
        Assert.True(annaAnim.IsCurrentState("Idle"));
        Assert.False(annaAnim.IsCurrentState("Interruption"));

        yield return new WaitForSeconds(1f);

        annaAnim.next.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // Start "Next State" transition NOT TO EXIT with interruption source "Current State".
        annaAnim.interruption.Fire();

        // Wait for the next-next frame. TriggerEx will be applied at the 2nd frame.
        yield return null;
        yield return null;

        Assert.False(annaAnim.IsCurrentState("Interruption"));
        Assert.True(annaAnim.IsCurrentState("Idle"));

        // Transition Duration is 0.05 so this transition will have finished.
        yield return new WaitForSeconds(0.05f);

        Assert.True(annaAnim.IsCurrentState("Interruption"));
        Assert.False(annaAnim.IsCurrentState("Idle"));

        yield return new WaitForSeconds(1f);

        annaAnim.run.Bool = true;

        yield return new WaitForSeconds(1f);

        Assert.True(annaAnim.IsCurrentState("Run"));

        annaAnim.run.Bool = false;
        annaAnim.attack.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // Transition Run -> Exit starts.
        Assert.True(annaAnim.IsCurrentTransition("Run", "Exit"));

        yield return null;

        // The transition switched to Idle -> Attack.
        Assert.True(annaAnim.IsCurrentTransition("Idle", "Attack"));

        yield return null;
        yield return null;

        // Transition Duration is 0.05 but the transition was switched to Attack.
        Assert.True(annaAnim.IsCurrentState("Run"));
        Assert.False(annaAnim.IsCurrentState("Attack"));

        // Transition Duration to Attack is 0.1 so this transition will have finished.
        yield return new WaitForSeconds(0.05f);

        Assert.False(annaAnim.IsCurrentState("Run"));
        Assert.True(annaAnim.IsCurrentState("Attack"));

        yield return new WaitForSeconds(1f);

        annaAnim.run.Bool = true;

        yield return new WaitForSeconds(1f);

        Assert.True(annaAnim.IsCurrentState("Run"));

        // Start "Next State" transition with interruption source "Current State".
        annaAnim.run.Bool = false;
        annaAnim.interruption.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // Transition Run -> Exit starts.
        Assert.True(annaAnim.IsCurrentTransition("Run", "Exit"));

        yield return null;

        // The transition switched to Idle -> Interruption.
        Assert.True(annaAnim.IsCurrentTransition("Idle", "Interruption"));

        yield return null;

        // The transition switched back to Run -> Exit. This behavior may be bug.
        Assert.True(annaAnim.IsCurrentTransition("Run", "Exit"));

        yield return null;

        // Transition Duration is 0.05 so this transition should have finished but not.
        Assert.True(annaAnim.IsCurrentState("Run"));
        Assert.False(annaAnim.IsCurrentState("Idle"));
        Assert.False(annaAnim.IsCurrentState("Interruption"));

        // Still running at the next frame.
        yield return null;

        Assert.True(annaAnim.IsCurrentState("Run"));
        Assert.False(annaAnim.IsCurrentState("Idle"));
        Assert.False(annaAnim.IsCurrentState("Interruption"));

        // Running finishes at the 2nd frame.
        yield return null;

        Assert.False(annaAnim.IsCurrentState("Run"));
        Assert.True(annaAnim.IsCurrentState("Idle"));
        Assert.False(annaAnim.IsCurrentState("Interruption"));

        yield return new WaitForSeconds(1f);

        annaAnim.run.Bool = true;

        yield return new WaitForSeconds(1f);

        Assert.True(annaAnim.IsCurrentState("Run"));

        // Start "Next State" transition with interruption source "Current State".
        annaAnim.run.Bool = false;
        annaAnim.interruptionAny.Fire();

        // Wait for the next-next frame. TriggerEx will be applied at the 2nd frame.
        yield return null;
        yield return null;

        // Transition Duration is 0.05 but the transition was switched to InterruptionAny.
        yield return new WaitForSeconds(0.05f);

        Assert.True(annaAnim.IsCurrentState("Run"));
        Assert.False(annaAnim.IsCurrentState("InterruptionAny"));

        // Transition Duration to InterruptionAny is 0.1 so this transition will have finished.
        yield return new WaitForSeconds(0.05f);

        Assert.False(annaAnim.IsCurrentState("Run"));
        Assert.True(annaAnim.IsCurrentState("InterruptionAny"));

        yield return new WaitForSeconds(1f);

        Object.Destroy(annaAnim.gameObject);
        Object.Destroy(mainCamera);
    }

    [Ignore("Only for spec confirmation.")]
    [UnityTest]
    /// <summary>
    /// Transition with "Next State" interruption source switches back to transition to "Exit" state.
    /// </summary>
    public IEnumerator _023_InterruptionSourceNextStateDoesNotSwitchBackToTransitionToExitState()
    {
        var mainCamera = Object.Instantiate(prefabCamera);
        var annaAnim = Object.Instantiate(Resources.Load<AnimatorTest>("Prefabs/Character/Anna2DFreeFormTest"), new Vector3(0f, 0f, -5f), Quaternion.identity);
        yield return new WaitForSeconds(1f);

        annaAnim.StartMoving(4f);

        yield return new WaitForSeconds(1f);

        // Current state is Move
        Assert.True(annaAnim.IsCurrentState("Move"));

        // Start "Next State" transition with interruption source "Current State".
        annaAnim.EndMoving();
        annaAnim.next.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // Transition Move -> Exit starts.
        Assert.True(annaAnim.IsCurrentTransition("Move", "Exit"));
        Assert.True(annaAnim.IsCurrentState("Move"));
        annaAnim.attack.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // The transition switched to Idle -> Next.
        Assert.True(annaAnim.IsCurrentTransition("Idle", "Next"));
        Assert.True(annaAnim.IsCurrentState("Move"));

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // The transition switched to Next -> Attack.
        Assert.True(annaAnim.IsCurrentTransition("Next", "Attack"));
        Assert.True(annaAnim.IsCurrentState("Move"));

        yield return new WaitForSeconds(0.05f);

        // Transition Duration to Attack is 0.05 so this transition will have finished.
        Assert.True(annaAnim.IsCurrentState("Attack"));

        yield return new WaitForSeconds(1f);

        annaAnim.StartMoving(4f);

        yield return new WaitForSeconds(1f);

        // Current state is Move
        Assert.True(annaAnim.IsCurrentState("Move"));

        // Start "Next State" transition with interruption source "Current State".
        annaAnim.EndMoving();
        annaAnim.next.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // Transition Move -> Exit starts.
        Assert.True(annaAnim.IsCurrentTransition("Move", "Exit"));
        Assert.True(annaAnim.IsCurrentState("Move"));
        annaAnim.interruption.Fire();

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // The transition switched to Idle -> Next.
        Assert.True(annaAnim.IsCurrentTransition("Idle", "Next"));
        Assert.True(annaAnim.IsCurrentState("Move"));

        // Wait for the next frame. The animation change will be applied at the next frame.
        yield return null;

        // The transition switched to Next -> Interruption.
        Assert.True(annaAnim.IsCurrentTransition("Next", "Interruption"));
        Assert.True(annaAnim.IsCurrentState("Move"));

        yield return null;

        // The transition switched back to Move -> Exit. This behavior may be bug.
        Assert.True(annaAnim.IsCurrentTransition("Move", "Exit"));

        yield return new WaitForSeconds(0.05f);

        // Transition Duration to Exit is 0.05 but still moving.
        Assert.True(annaAnim.IsCurrentState("Move"));

        yield return null;

        // The transition finishes on next frame.
        Assert.True(annaAnim.IsCurrentState("Idle"));

        yield return new WaitForSeconds(1f);

        Object.Destroy(annaAnim.gameObject);
        Object.Destroy(mainCamera);
    }
}
