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
        if (upArrow != null) flick.DragUp.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDragUp(ratio)).AddTo(this);
        if (downArrow != null) flick.DragDown.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDragDown(ratio)).AddTo(this);
        if (rightArrow != null) flick.DragRight.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDragRight(ratio)).AddTo(this);
        if (leftArrow != null) flick.DragLeft.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDragLeft(ratio)).AddTo(this);

        flick.ReleaseSubject.Subscribe(_ => OnRelease()).AddTo(this);
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

    private void OnDragUp(float dragRatio)
    {
        SetUIsActive(moveArrows, dragRatio < 0.5f, upArrow);

        OnDrag(dragRatio);
    }

    private void OnDragDown(float dragRatio)
    {
        SetUIsActive(moveArrows, dragRatio < 0.5f, downArrow);

        OnDrag(dragRatio);
    }

    private void OnDragRight(float dragRatio)
    {
        SetUIsActive(moveArrows, dragRatio < 0.5f, rightArrow);

        OnDrag(dragRatio);
    }

    private void OnDragLeft(float dragRatio)
    {
        SetUIsActive(moveArrows, dragRatio < 0.5f, leftArrow);

        OnDrag(dragRatio);
    }

    public void OnDrag(float dragRatio)
    {
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

    public void Activate()
    {
        isPressed = false;

        gameObject.SetActive(true);

        flick.FadeIn(0.2f).Play();
        SetUIsActive(stopArrows, true);
    }

    public void Inactivate()
    {
        isPressed = false;
        flick.FadeOut(0.2f, null, () => gameObject.SetActive(false)).Play();

        SetUIsActive(stopArrows, false);
        SetUIsActive(moveArrows, false);
    }

    public void SetActive(bool isActive)
    {
        if (isActive)
        {
            Activate();
        }
        else
        {
            Inactivate();
        }
    }
}
