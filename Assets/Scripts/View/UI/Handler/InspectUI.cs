using UniRx;
using System;

public class InspectUI : PointerDownUI
{
    protected MessageWall message;
    protected Furniture furniture;

    public IObservable<MessageData> OnInspectMessage => PressObservable.Where(_ => message != null).Select(_ => message.data);
    public IObservable<Furniture> OnInspectStructure => PressObservable.Where(_ => furniture != null).Select(_ => furniture);

    public void SetActive(MessageWall message, IDirection dir, bool isFighting = false)
    {
        base.SetActive(message.IsReadable(dir), isFighting);
        if (isActive)
        {
            this.message = message;
            this.furniture = null;
        }
    }

    public void SetActive(Furniture furniture, IDirection dir, bool isFighting = false)
    {
        base.SetActive(true, isFighting);
        if (isActive)
        {
            this.furniture = furniture;
            this.message = null;
        }
    }
}
