using UnityEngine;
using UnityEngine.UI;

public class LifeGauge : MonoBehaviour
{
    [SerializeField] private Image fillImage = default;

    private RectTransform _parentRectTransform;
    private Camera _camera;
    private MobStatus _status;

    // Start is called before the first frame update
    private void Update()
    {
        Refresh();
    }

    // Update is called once per frame
    public void Initialize(RectTransform parentRectRectTransform, Camera camera, MobStatus status)
    {
        _parentRectTransform = parentRectRectTransform;
        _camera = camera;
        _status = status;
        Refresh();
    }

    private void Refresh()
    {
        fillImage.fillAmount = _status.Life / _status.LifeMax;
        Vector2 screenPoint = _camera.WorldToScreenPoint(_status.transform.position);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, screenPoint, null, out localPoint);
        transform.localPosition = localPoint + new Vector2(0, 80);
    }
}
