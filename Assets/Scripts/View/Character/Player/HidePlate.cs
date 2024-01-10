using UnityEngine;
using DG.Tweening;

public class HidePlate : SpawnObject<HidePlate>
{
    /// <summary>
    /// Plate shape type
    /// </summary>
    public Plate plate { get; protected set; } = Plate.NONE;

    private Tween spawnTween = null;
    private Tween expandTween = null;
    private Tween removeTween = null;
    private Tween clearTween = null;
    private Tween moveTween = null;

    private Tween currentTween = null;
    private Tween removeTimer = null;

    private Material material;
    private Renderer plateRenderer;

    private Color color
    {
        get
        {
            return material.color;
        }
        set
        {
            material.color = value;
        }
    }

    private void SetAlpha(float alpha)
    {
        color = new Color(color.r, color.g, color.b, alpha);
    }

    private Tween ToAlpha(float alpha, float duration)
        => DOTween.ToAlpha(() => color, value => color = value, alpha, duration)
            .SetEase(Ease.Linear).SetUpdate(false).AsReusable(gameObject);

    private Tween FadeIn(float duration)
    {
        SetAlpha(0f);
        return ToAlpha(1f, duration);
    }
    private Tween FadeOut(float duration)
    {
        SetAlpha(1f);
        return ToAlpha(0f, duration).OnComplete(() => gameObject.SetActive(false));
    }

    private void PlayTween(Tween nextTween)
    {
        currentTween?.Complete();
        nextTween?.Rewind();
        currentTween = nextTween.Play();
    }

    private float[] angle = new float[0b10000];
    protected void Awake()
    {
        plateRenderer = GetComponent<Renderer>();

        angle[(int)Plate.A] = angle[(int)Plate.AB] = angle[(int)Plate.ABC] = angle[(int)Plate.ABCD] = angle[(int)Plate.AD] = 0f;
        angle[(int)Plate.B] = angle[(int)Plate.BD] = angle[(int)Plate.ABD] = angle[(int)Plate.BC] = 90f;
        angle[(int)Plate.D] = angle[(int)Plate.CD] = angle[(int)Plate.BCD] = 180f;
        angle[(int)Plate.C] = angle[(int)Plate.AC] = angle[(int)Plate.ACD] = -90f;
    }

    public HidePlate SetMaterial(Material material)
    {
        currentTween?.Complete();
        removeTimer?.Complete();

        this.material = Util.SwitchMaterial(plateRenderer, material);

        spawnTween = spawnTween ?? FadeIn(0.01f);
        expandTween = expandTween ?? FadeIn(0.2f);
        removeTween = removeTween ?? FadeOut(0.3f);
        clearTween = clearTween ?? FadeOut(0.25f);
        moveTween = moveTween ?? FadeOut(0.4f);

        return this;
    }

    public override HidePlate OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.01f)
    {
        transform.position = pos;

        gameObject.SetActive(true);

        // FIXME: This dirty switching logic is forced by the signature of OnSpawn() fixed by SpawnObject.
        switch (duration)
        {
            case 0.01f:
                PlayTween(spawnTween);
                break;

            case 0.2f:
                PlayTween(expandTween);
                break;

            default:
                SetAlpha(1f);
                break;
        }

        return this;
    }

    public HidePlate SetPlateType(Plate plate, Vector4 rotationMatrix)
    {
        this.plate = plate;
        material.SetVector("_Rotate", rotationMatrix);
        return this;
    }

    public void Remove() => PlayTween(removeTween);
    public void Move() => PlayTween(moveTween);
    public void Clear() => PlayTween(clearTween);

    public void Hide() => plateRenderer.enabled = false;
    public void Show() => plateRenderer.enabled = true;

    public void ReplaceTimer(float timeSec, HidePlate replaceWith = null)
    {
        removeTimer = DOVirtual.DelayedCall(timeSec, RemoveImmediately, false);

        if (replaceWith != null)
        {
            replaceWith.Hide();
            removeTimer.OnComplete(replaceWith.Show);
        }

        removeTimer.Play();
    }

    public void RemoveImmediately()
    {
        currentTween?.Complete();
        Inactivate();
    }
}
