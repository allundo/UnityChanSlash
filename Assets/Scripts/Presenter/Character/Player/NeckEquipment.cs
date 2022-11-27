using UnityEngine;

public class NeckEquipment : BodyEquipment
{
    [SerializeField] private Vector3 position = default;
    [SerializeField] private Vector3 rotate = default;

    protected override Vector3 GetLocalPosition(EquipmentSource source) => position;
    protected override Vector3 GetLocalRotation(EquipmentSource source) => rotate;
}