using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MoveUI : MonoBehaviour
{
    [SerializeField] protected MoveButton moveButton = default;

    protected RectTransform rectTransform;
    protected Image image;
    protected Vector2 defaultSize;

    protected bool isActive = false;
    private Tween buttonFade = null;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        defaultSize = rectTransform.sizeDelta;
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
        image.raycastTarget = true;

        buttonFade?.Kill();
        gameObject.SetActive(true);
        buttonFade = moveButton.FadeIn(0.2f).Play();
    }

    public void Inactivate(bool isFighting = false)
    {
        moveButton.SetFightingPos(isFighting);

        if (!isActive) return;

        isActive = false;
        image.raycastTarget = false;

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
