using UnityEngine;
using DG.Tweening;

public class MoveUI : MonoBehaviour
{
    [SerializeField] protected MoveButton moveButton = default;

    protected RectTransform rectTransform;
    protected Vector2 defaultSize;

    protected RaycastHandler raycastMoveButton;

    protected bool isActive = false;
    private Tween buttonFade = null;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultSize = rectTransform.sizeDelta;
        raycastMoveButton = new RaycastHandler(moveButton.gameObject);
    }

    protected virtual void Start()
    {
        moveButton.Inactivate();
        gameObject.SetActive(false);
    }

    public void Resize(float ratioX, float ratioY)
    {
        rectTransform.sizeDelta = new Vector2(ratioX * defaultSize.x, ratioY * defaultSize.y);
    }

    public void Activate(bool isFighting = false)
    {
        moveButton.SetFightingPos(isFighting);

        if (isActive) return;

        isActive = true;

        buttonFade?.Kill();
        gameObject.SetActive(true);
        buttonFade = moveButton.FadeIn(0.2f).Play();
    }

    public void Inactivate(bool isFighting = false)
    {
        moveButton.SetFightingPos(isFighting);

        if (!isActive) return;

        isActive = false;

        buttonFade?.Kill();
        buttonFade = moveButton.FadeOut(0.2f, null, () => gameObject.SetActive(false)).Play();
    }

    public void SetActive(bool value, bool isFighting = false)
    {
        if (value)
        {
            Activate(isFighting);
        }
        else
        {
            Inactivate(isFighting);
        }

    }
}
