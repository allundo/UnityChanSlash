using UnityEngine;
using UniRx;

public class ActiveMessageController : MonoBehaviour
{
    [SerializeField] protected ActiveMessageBox messageBox = default;
    [SerializeField] protected SDIcon sdIcon = default;

    void Start()
    {
        Observable.Merge(messageBox.CloseSignal, sdIcon.CloseSignal)
            .Subscribe(_ => Close())
            .AddTo(this);
    }

    private void Close()
    {
        sdIcon.Inactivate();
        messageBox.Inactivate();
    }

    public void InputMessageData(ActiveMessageData messageData)
    {
        sdIcon.Activate(messageData);
        messageBox.Activate(messageData);
    }
}
