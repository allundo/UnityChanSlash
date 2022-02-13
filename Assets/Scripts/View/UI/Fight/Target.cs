using DG.Tweening;
using UnityEngine;

public class Target : FadeActivate
{
    [SerializeField] TargetCenter center = default;
    [SerializeField] TargetCorner corner = default;

    private EnemyStatus status;
    private RectTransform rectTransform;
    public Vector2 screenPos => rectTransform.position;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject, 0.2f);
        rectTransform = GetComponent<RectTransform>();
        Inactivate();
    }

    void Update()
    {
        if (status != null)
        {
            rectTransform.position = Camera.main.WorldToScreenPoint(status.corePos);
        }
    }

    public void FadeActivate(EnemyStatus status)
    {
        this.status = status;
        FadeIn(0.5f).Play();
    }

    protected override void OnFadeIn()
    {
        corner.FadeActivate();
        center.FadeActivate();
    }

    public void FadeInactivate()
    {
        FadeOut(0.1f).Play();
    }

    protected override void OnFadeOut()
    {
        corner.FadeInactivate();
        center.FadeInactivate();
    }
}