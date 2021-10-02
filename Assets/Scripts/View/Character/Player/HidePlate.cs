using UnityEngine;
using DG.Tweening;

public class HidePlate : FadeActivate, ISpawnObject<HidePlate>
{
    public Plate plate { get; protected set; } = Plate.NONE;

    private Tween fadeInTween = null;
    private Tween removeTween = null;

    protected override void Awake()
    {
        fade = new FadeMaterialColor(gameObject, 1f);
    }

    public HidePlate OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.01f)
    {
        transform.position = pos;

        gameObject.SetActive(true);
        removeTween?.Kill();
        fadeInTween = FadeIn(duration).SetEase(Ease.Linear).Play();

        return this;
    }

    public HidePlate SetPlate(Plate plate)
    {
        this.plate = plate;
        return this;
    }

    public void Remove(float duration = 0.3f)
    {
        fade.KillTweens();
        fadeInTween?.Kill();

        removeTween = FadeOut(duration).SetEase(Ease.Linear).Play();
    }
}
