using UnityEngine;
using NUnit.Framework;

public class TestNestedMonoBehaviour : TestMonoBehaviour
{
    private TestMonoBehaviour prefab;
    private TestMonoBehaviour child;

    public bool isChildAwakeCalled => child != null && child.isAwakeCalled;
    public bool isChildStartCalled => child != null && child.isStartCalled;
    public bool isChildUpdateCalled => child != null && child.isUpdateCalled;

    protected override void Awake()
    {
        base.Awake();
        prefab = Resources.Load<TestMonoBehaviour>("Prefabs/TestGameObject");
    }

    protected override void Start()
    {
        base.Start();
        child = Object.Instantiate(prefab, new Vector3(-0.25f, 0f, 0f), Quaternion.identity);
        Assert.IsTrue(child.isAwakeCalled);
        Assert.IsFalse(child.isStartCalled);
        Assert.IsFalse(child.isUpdateCalled);
    }

    void OnDestroy()
    {
        Destroy(child?.gameObject);
    }
}
