using UnityEngine;
using DG.Tweening;
using UniRx;
using System;

[RequireComponent(typeof(Collider))]
public class DoorOpener : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        IHandleStructure targetStructure = other.GetComponent<HandleStructure>();

        if (null == targetStructure) return;

        targetStructure.Handle();
    }

    public IObservable<Unit> Shoot(Vector3 destPos)
    {
        return transform.DOMove(destPos, 0.5f).SetEase(Ease.Linear).OnCompleteAsObservable(Unit.Default);
    }
}