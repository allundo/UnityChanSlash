using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;
using UniRx;

public class MessageController : FadeActivateImage, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected FadeActivateImage window = default;
    [SerializeField] protected CharacterUI characterUI = default;
    [SerializeField] protected TextHandler textHandler = default;

    protected bool isUIActive = false;

    protected Tween activateTween = null;

    protected override void Awake()
    {
        fade = new FadeTweenImage(gameObject, 0.25f, true);
    }

    protected override void Start()
    {
        textHandler.Sentence.Subscribe(
            faceID => characterUI.DispFace(faceID),
            err => Debug.Log(err),
            () => CloseMessage()
        ).AddTo(this);

        Inactivate();
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isActive) return;

        activateTween?.Complete(true);

        textHandler.TapNext();
    }

    public void InputMessageData(MessageData data)
    {
        activateTween =
            DOTween.Sequence()
            .Join(
                FadeIn(
                    0.5f,
                    () => GameManager.Instance.Pause(),
                    () =>
                    {
                        textHandler.InputMessageData(data);
                        activateTween = null;
                    }
                )
            )
            .Join(window.FadeIn(0.5f))
            .SetUpdate(true)
            .Play();

        characterUI.InputFaceIDs(data.faces);
    }

    private void CloseMessage()
    {
        FadeOut(0.5f, null, () => GameManager.Instance.Resume()).Play();
        window.FadeOut(0.5f).Play();
        characterUI.DispFace(FaceID.NONE);
    }
}
