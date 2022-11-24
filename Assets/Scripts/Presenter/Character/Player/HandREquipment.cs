using UnityEngine;

public class HandREquipment : MonoBehaviour
{
    protected GameObject equipment = null;

    public void Equip(EquipmentSource source)
    {
        if (equipment != null) Destroy(equipment);
        if (source.prefabEquipment != null) SetTransform(source);
    }

    protected virtual void SetTransform(EquipmentSource source)
    {
        equipment = Util.Instantiate(source.prefabEquipment, transform);
        equipment.transform.localPosition = source.handRPosition;
        equipment.transform.localRotation = Quaternion.Euler(source.handRRotate);
    }
}