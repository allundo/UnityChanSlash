using UnityEngine;

public class HandREquipment : BodyEquipment
{
    protected override Vector3 GetLocalPosition(EquipmentSource source) => source.handRPosition;
    protected override Vector3 GetLocalRotation(EquipmentSource source) => source.handRRotate;
}