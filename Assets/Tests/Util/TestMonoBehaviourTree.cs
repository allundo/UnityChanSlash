using UnityEngine;

public class TestMonoBehaviourTree : TestMonoBehaviour
{
    [SerializeField] private TestMonoBehaviour child = default;

    public bool isChildAwakeCalled => child != null && child.isAwakeCalled;
    public bool isChildStartCalled => child != null && child.isStartCalled;
    public bool isChildUpdateCalled => child != null && child.isUpdateCalled;
}
