using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;
using UniRx;

public class MessageController : FadeActivate, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected FadeActivate window = default;
    [SerializeField] protected CharacterUI characterUI = default;
    [SerializeField] protected TextHandler textHandler = default;

    protected bool isUIActive = false;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.25f, true);
    }

    protected override void Start()
    {
        base.Inactivate(0f);

        textHandler.subject.Subscribe(
            faceID => characterUI.DispFace(faceID),
            err => Debug.Log(err),
            () => CloseMessage()
        ).AddTo(this);

    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isUIActive) return;
        textHandler.TapNext();
    }

    public void InputMessageData(MessageData data)
    {
        Activate(0.5f, () => textHandler.InputMessageData(data));
        window.Activate(0.5f);
        characterUI.InputFaceIDs(data.faces);
    }

    private void CloseMessage()
    {
        Inactivate(0.5f);
        window.Inactivate(0.5f);
        characterUI.DispFace(FaceID.NONE);
    }

    public override Tween Activate(float duration = 1f, TweenCallback onComplete = null)
    {
        onComplete = onComplete ?? (() => { });

        GameManager.Instance.Pause(true);

        return base.Activate(duration, () =>
        {
            isUIActive = true;
            onComplete();
        });
    }

    public override Tween Inactivate(float duration = 1f, TweenCallback onComplete = null)
    {
        onComplete = onComplete ?? (() => { });

        isUIActive = false;

        return base.Inactivate(duration, () =>
        {
            onComplete();
            GameManager.Instance.Resume();
        });
    }
}
