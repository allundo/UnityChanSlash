using UnityEngine;
using UniRx;
using NUnit.Framework;

public class TestUniRxTree : MonoBehaviour
{
    private TestMonoBehaviourTree prefab;
    private TestMonoBehaviourTree tree;
    private IReactiveProperty<bool> testRP = new ReactiveProperty<bool>(false);
    protected virtual void Awake()
    {
        prefab = Resources.Load<TestMonoBehaviourTree>("Prefabs/TestMonoBehaviourTree");
        testRP.Value = true;
    }

    protected virtual void Start()
    {
        testRP.Subscribe(value => InstantiateTest(value)).AddTo(this);
    }

    private void InstantiateTest(bool value)
    {
        Assert.IsTrue(value);

        tree = Object.Instantiate(prefab, new Vector3(-0.25f, 0f, 0f), Quaternion.identity, transform);
        Assert.IsTrue(tree.isChildAwakeCalled);
        Assert.IsFalse(tree.isChildStartCalled);
        Assert.IsFalse(tree.isChildUpdateCalled);
    }

    void OnDestroy()
    {
        Destroy(tree?.gameObject);
    }
}
