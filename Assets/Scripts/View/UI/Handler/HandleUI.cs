using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using Coffee.UIExtensions;
using DG.Tweening;

public class HandleUI : MonoBehaviour
{
    [SerializeField] protected UIParticle[] stopArrowPrefabs = default;
    protected UIParticle[] stopArrows;

    [SerializeField] protected UIParticle upArrowPrefab = default;
    [SerializeField] protected UIParticle downArrowPrefab = default;
    [SerializeField] protected UIParticle rightArrowPrefab = default;
    [SerializeField] protected UIParticle leftArrowPrefab = default;
    protected UIParticle upArrow = default;
    protected UIParticle downArrow = default;
    protected UIParticle rightArrow = default;
    protected UIParticle leftArrow = default;
    protected UIParticle[] moveArrows;

    [SerializeField] private FlickInteraction flick = default;

    private bool isPressed = false;

    void Awake()
    {
        InstantiateAllPrefabs();

        SetUIsActive(stopArrows, false);
        SetUIsActive(moveArrows, false);
    }

    void Start()
    {
        if (upArrow != null) flick.DragUp.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDrag(ratio, upArrow)).AddTo(this);
        if (downArrow != null) flick.DragDown.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDrag(ratio, downArrow)).AddTo(this);
        if (rightArrow != null) flick.DragRight.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDrag(ratio, rightArrow)).AddTo(this);
        if (leftArrow != null) flick.DragLeft.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDrag(ratio, leftArrow)).AddTo(this);

        flick.IsReleased.Subscribe(_ => OnRelease()).AddTo(this);
    }

    private void InstantiateAllPrefabs()
    {
        stopArrows = stopArrowPrefabs.Select(arrow => InstantiateUI(arrow)).ToArray();

        if (upArrowPrefab != null) upArrow = InstantiateUI(upArrowPrefab);
        if (downArrowPrefab != null) downArrow = InstantiateUI(downArrowPrefab);
        if (rightArrowPrefab != null) rightArrow = InstantiateUI(rightArrowPrefab);
        if (leftArrowPrefab != null) leftArrow = InstantiateUI(leftArrowPrefab);

        moveArrows = new[] { upArrow, downArrow, rightArrow, leftArrow };
    }

    /// <summary>
    /// UGUI objects must be set as children of Canvas object
    /// </summary>
    private T InstantiateUI<T>(T prefabUI) where T : UnityEngine.Object
        => Instantiate(prefabUI, transform, false);

    private void SetUIsActive(MaskableGraphic[] sequence, bool isActive)
    {
        sequence.ForEach(ui => ui.gameObject.SetActive(isActive), null);
    }
    private void SetUIsActive(MaskableGraphic[] sequence, bool isActive, MaskableGraphic exceptFor)
    {
        sequence.ForEach(ui => ui.gameObject.SetActive(isActive), null, exceptFor);
    }

    public void OnDrag(float dragRatio, UIParticle moveArrow)
    {
        SetUIsActive(moveArrows, dragRatio < 0.5f, moveArrow);

        if (isPressed) return;

        isPressed = true;

        SetUIsActive(stopArrows, false);
        SetUIsActive(moveArrows, true);
    }

    public void OnRelease()
    {
        isPressed = false;

        SetUIsActive(stopArrows, true);
        SetUIsActive(moveArrows, false);
    }

    public void Activate(float duration = 0.2f)
    {
        isPressed = false;

        gameObject.SetActive(true);

        flick.FadeIn(duration).Play();
        SetUIsActive(stopArrows, true);
    }

    public void Inactivate(float duration = 0.2f)
    {
        isPressed = false;
        flick.FadeOut(duration, null, () => gameObject.SetActive(false)).Play();

        SetUIsActive(stopArrows, false);
        SetUIsActive(moveArrows, false);
    }

    public void SetActive(bool isActive, float duration = 0.2f)
    {
        if (isActive)
        {
            Activate(duration);
        }
        else
        {
            Inactivate(duration);
        }
    }
}
