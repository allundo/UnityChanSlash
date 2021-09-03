using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private Sprite[] face = default;

    private Image image;
    private FadeTween fade;
    private UITween uiTween;
    private Tween faceTween = null;
    private Vector2 faceMoveOrigin;
    private FaceID[] faceIDs;

    void Awake()
    {
        image = GetComponent<Image>();

        fade = new FadeTween(image, 1f, true);
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

        if (faceID == FaceID.NONE) return;

        image.sprite = face[(int)faceID];

        faceTween?.Kill();

        faceTween =
            DOTween.Sequence()
                .AppendCallback(() => uiTween.SetPos(faceMoveOrigin))
                .Join(fade.In(0.5f))
                .Join(uiTween.MoveBack(0.5f))
                .SetUpdate(true)
                .Play();
    }
}
