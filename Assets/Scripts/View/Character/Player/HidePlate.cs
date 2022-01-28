using UnityEngine;
using DG.Tweening;

public class HidePlate : FadeActivate, ISpawnObject<HidePlate>
{
    /// <summary>
    /// Plate shape type
    /// </summary>
    public Plate plate { get; protected set; } = Plate.NONE;

    private Tween fadeInTween = null;
    private Tween removeTween = null;

    private Material material;
    private Renderer plateRenderer;

    private float[] angle = new float[0b10000];
    protected override void Awake()
    {
        plateRenderer = GetComponent<Renderer>();
        fade = new FadeMaterialColor(gameObject, 1f);

        angle[(int)Plate.A] = angle[(int)Plate.AB] = angle[(int)Plate.ABC] = angle[(int)Plate.ABCD] = angle[(int)Plate.AD] = 0f;
        angle[(int)Plate.B] = angle[(int)Plate.BD] = angle[(int)Plate.ABD] = angle[(int)Plate.BC] = 90f;
        angle[(int)Plate.D] = angle[(int)Plate.CD] = angle[(int)Plate.BCD] = 180f;
        angle[(int)Plate.C] = angle[(int)Plate.AC] = angle[(int)Plate.ACD] = -90f;
    }

    public HidePlate SetMaterial(Material material)
    {
        this.material = Util.SwitchMaterial(plateRenderer, material);
        (fade as FadeMaterialColor).SetMaterial(this.material);
        return this;
    }

    public HidePlate OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.01f)
    {
        transform.position = pos;

        gameObject.SetActive(true);
        removeTween?.Kill();

        if (duration > 0f)
        {
            fadeInTween = FadeIn(duration, null, null, false).SetEase(Ease.Linear).Play();
        }
        else
        {
            fade.SetAlpha(1f);
        }

        return this;
    }

    public void SetAlpha(float alpha) => fade.SetAlpha(alpha);

    public HidePlate SetPlateType(Plate plate, Vector4 rotationMatrix)
    {
        this.plate = plate;
        material.SetVector("_Rotate", rotationMatrix);
        return this;
    }

    /// <summary>
    /// Applies fade-out to inactivation.
    /// </summary>
    /// <param name="duration">Fade-out duration [sec]</param>
    public void Remove(float duration = 0.3f)
    {
        fade.KillTweens();
        fadeInTween?.Kill();

        removeTween = FadeOut(duration).SetEase(Ease.Linear).Play();
    }

    public Tween RemoveTimer(float timeSec)
    {
        return DOVirtual.DelayedCall(timeSec, () =>
        {
            fade.KillTweens();
            fadeInTween?.Kill();
            Inactivate();
        }, false);
    }

    public void RemoveImmediately()
    {
        fadeInTween?.Kill();
        removeTween?.Kill();
        Inactivate();
    }
}
