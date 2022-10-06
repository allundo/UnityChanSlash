using UnityEngine;

public class TestMonoBehaviour : MonoBehaviour
{
    public bool isAwakeCalled = false;
    public bool isStartCalled = false;
    public bool isUpdateCalled = false;

    protected virtual void Awake()
    {
        isAwakeCalled = true;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        isStartCalled = true;
    }

    // Update is called once per frame
    void Update()
    {
        isUpdateCalled = true;
    }
}
