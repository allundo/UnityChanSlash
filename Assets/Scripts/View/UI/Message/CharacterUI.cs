using UnityEngine;
using DG.Tweening;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private Sprite[] face = default;

    private FadeTween fade;
    private UITween uiTween;
    private Tween faceTween = null;
    private Vector2 faceMoveOrigin;
    private FaceID[] faceIDs;

    void Awake()
    {
        fade = new FadeTween(gameObject, 1f, true);
        uiTween = new UITween(gameObject, true);

        fade.SetAlpha(0f);
        faceMoveOrigin = uiTween.defaultPos + new Vector2(50f, 0);
    }

    public void InputFaceIDs(FaceID[] faceIDs)
    {
        this.faceIDs = faceIDs;
    }

    public void DispFace(FaceID faceID = FaceID.NONE)
    {
        fade.SetAlpha(0f);
        faceTween?.Kill();

        if (faceID == FaceID.NONE) return;

        fade.sprite = face[(int)faceID];

        faceTween =
            DOTween.Sequence()
                .AppendCallback(() => uiTween.SetPos(faceMoveOrigin))
                .Join(fade.In(0.5f))
                .Join(uiTween.MoveBack(0.5f))
                .SetUpdate(true)
                .Play();
    }
}
