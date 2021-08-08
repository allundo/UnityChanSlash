using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private Sprite[] face = default;

    private Image image;
    private FadeTween fade;
    private UITween uiTween;
    private Vector2 faceMoveOrigin;
    private FaceID[] faceIDs;

    private Tween dispFaceTween;
    void Awake()
    {
        image = GetComponent<Image>();

        fade = new FadeTween(image, 1f, true);
        uiTween = new UITween(gameObject, true);

        fade.SetAlpha(0f);
        faceMoveOrigin = uiTween.defaultPos + new Vector2(50f, 0);

        /*
                dispFaceTween =
                    DOTween.Sequence()
                        .Join(fade.In(0.5f))
                        .Join(uiTween.MoveBack(0.5f))
                        .AsReusable(gameObject);
                        */
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

        fade.In(0.5f).Play();
        uiTween.SetPos(faceMoveOrigin);
        uiTween.MoveBack(0.5f).Play();
        // dispFaceTween.Restart();

    }
}
