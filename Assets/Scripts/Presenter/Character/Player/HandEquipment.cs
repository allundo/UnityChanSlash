using UnityEngine;

public class HandEquipment : MonoBehaviour
{
    private GameObject equipment = null;

    public void Equip(EquipmentSource source)
    {
        if (equipment != null) Destroy(equipment);

        if (source.prefabEquipment != null)
        {
            equipment = Util.Instantiate(source.prefabEquipment, transform);
            equipment.transform.localPosition = source.handRPosition;
            equipment.transform.localRotation = Quaternion.Euler(source.handRRotate);
        }
    }
}