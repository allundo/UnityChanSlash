using UnityEngine;
using UniRx;

public class ScreenRotateHandler : MonoBehaviour
{
    private IReactiveProperty<DeviceOrientation> currentOrientation = new ReactiveProperty<DeviceOrientation>();
    public IReadOnlyReactiveProperty<DeviceOrientation> Orientation => currentOrientation;

    void Awake()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortraitUpsideDown = false;
    }

    void Update()
    {
        currentOrientation.Value = GetOrientation();
    }

    private DeviceOrientation GetOrientation()
         => Screen.width < Screen.height ? DeviceOrientation.Portrait : DeviceOrientation.LandscapeRight;
}
