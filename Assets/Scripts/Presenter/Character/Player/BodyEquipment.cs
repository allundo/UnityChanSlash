using UnityEngine;

public abstract class BodyEquipment : MonoBehaviour
{
    protected GameObject equipment = null;

    public void Equip(EquipmentSource source)
    {
        if (equipment != null) Destroy(equipment);
        if (source.prefabEquipment != null) SetTransform(source);
    }

    protected void SetTransform(EquipmentSource source)
    {
        equipment = Util.Instantiate(source.prefabEquipment, transform);
        equipment.transform.localPosition = GetLocalPosition(source);
        equipment.transform.localRotation = Quaternion.Euler(GetLocalRotation(source));
    }

    protected abstract Vector3 GetLocalPosition(EquipmentSource source);
    protected abstract Vector3 GetLocalRotation(EquipmentSource source);
}