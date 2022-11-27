using UnityEngine;

public class HandLEquipment : BodyEquipment
{
    protected override Vector3 GetLocalPosition(EquipmentSource source) => source.handLPosition;
    protected override Vector3 GetLocalRotation(EquipmentSource source) => source.handLRotate;
}