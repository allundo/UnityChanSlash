using UniRx;
using System;

public class InspectUI : PointerDownUI
{
    protected MessageWall message;
    public IObservable<MessageData[]> OnInspectMessage => PressObservable.Select(_ => message.Read);

    public void SetActive(MessageWall message, IDirection dir, bool isFighting = false)
    {
        base.SetActive(message.IsReadable(dir), isFighting);
        if (isActive) this.message = message;
    }
}
