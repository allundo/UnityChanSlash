using UnityEngine;
using UnityEngine.UI;

public class DisplayEdge : MonoBehaviour
{
    [SerializeField] private Image top = default;
    [SerializeField] private Image bottom = default;
    [SerializeField] private Image left = default;
    [SerializeField] private Image right = default;

    public void ResetOrientation(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                top.enabled = bottom.enabled = true;
                left.enabled = right.enabled = false;
                break;

            case DeviceOrientation.LandscapeRight:
                top.enabled = bottom.enabled = false;
                left.enabled = right.enabled = true;
                break;
        }
    }
}