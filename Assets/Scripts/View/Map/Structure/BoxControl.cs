using UnityEngine;
using DG.Tweening;

public class BoxControl : HandleStructure
{
    [SerializeField] private Transform lidTf = default;

    protected override Tween GetDoorHandle(bool isOpen)
    {
        return lidTf.DOLocalRotate(new Vector3(isOpen ? -90f : 0f, 0, 0), 0.5f)
                .OnComplete(() => handleState.TransitToNextState());
    }
}
