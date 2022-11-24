using UnityEngine;

public class HandLEquipment : HandREquipment
{
    protected override void SetTransform(EquipmentSource source)
    {
        equipment = Util.Instantiate(source.prefabEquipment, transform);
        equipment.transform.localPosition = source.handLPosition;
        equipment.transform.localRotation = Quaternion.Euler(source.handLRotate);
    }
}