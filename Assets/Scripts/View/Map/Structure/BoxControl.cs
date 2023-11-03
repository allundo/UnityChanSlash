using UnityEngine;
using DG.Tweening;

public class BoxControl : HandleStructure
{
    [SerializeField] private Transform lidTf = default;

    protected override Tween GetHandleTween(bool isOpen)
    {
        return DOTween.Sequence()
                .AppendCallback(() => GameManager.Instance.PlaySnd(isOpen ? SNDType.BoxOpen : SNDType.BoxClose, transform.position))
                .Append(lidTf.DOLocalRotate(new Vector3(isOpen ? -90f : 0f, 0, 0), 0.6f))
                .AppendCallback(() => handleState.TransitToNextState());
    }

    protected override void ForceOpen()
    {
        lidTf.rotation *= Quaternion.Euler(-90f, 0f, 0f);
        handleState.TransitToNextState();
    }
}
