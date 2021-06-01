using UnityEngine;
using System.Collections;

public class DoorTransparent : MonoBehaviour
{
    public void OnDoorStay(Collider collider)
    {
        DoorControl targetDoor = collider.GetComponent<DoorControl>();
        if (null == targetDoor) return;

        float distance = (targetDoor.transform.position - transform.position).magnitude;
        targetDoor.SetAlpha(distance);
    }

    public void OnDoorExit(Collider collider)
    {
        DoorControl targetDoor = collider.GetComponent<DoorControl>();
        if (null == targetDoor) return;
        targetDoor.ResetAlpha();
    }
}