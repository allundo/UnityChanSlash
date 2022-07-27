using UnityEngine;
using UniRx;

public class ActiveMessageController : SingletonMonoBehaviour<ActiveMessageController>
{
    [SerializeField] protected ActiveMessageBox messageBox = default;
    [SerializeField] protected SDIcon sdIcon = default;

    void Start()
    {
        Observable.Merge(messageBox.CloseSignal, sdIcon.CloseSignal)
            .Subscribe(_ => Close())
            .AddTo(this);
    }

    public void Close()
    {
        sdIcon.Inactivate();
        messageBox.Inactivate();
    }

    public void InputMessageData(string message) => InputMessageData(new ActiveMessageData(message));
    public void InputMessageData(ActiveMessageData messageData)
    {
        sdIcon.Activate(messageData);
        messageBox.Activate(messageData);
    }
}
