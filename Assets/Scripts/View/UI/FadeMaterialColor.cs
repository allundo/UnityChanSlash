using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeMaterialColor : FadeTween
{
    protected Material material;
    protected GameObject gameObject;

    public FadeMaterialColor(MaskableGraphic image, float maxAlpha = 1f, bool isValidOnPause = false)
        : base(image, maxAlpha, isValidOnPause)
    {
        gameObject = image.gameObject;

        if (image.material == null) Debug.LogError("GameObject " + gameObject.name + " は Material を持っていません", gameObject);

        material = new Material(image.material);
        image.material = material;

        if (material != null) defaultColor = material.color;
    }

    public FadeMaterialColor(GameObject gameObject, float maxAlpha = 1f, bool isValidOnPause = false)
        : base(gameObject.GetComponent<MaskableGraphic>(), maxAlpha, isValidOnPause)
    {
        this.gameObject = gameObject;

        // GameObject is MaskableGraphic
        if (image != null)
        {
            material = new Material(image.material);
            image.material = material;
        }
        // GameObject has Renderer
        else
        {
            material = gameObject.GetComponent<Renderer>()?.material;
            if (material == null) Debug.LogError("GameObject " + gameObject.name + " は Material を持っていません", gameObject);
        }

        if (material != null) defaultColor = material.color;
    }

    public override Color color
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

    public override void SetActive(bool isActive = true)
    {
        gameObject.SetActive(isActive);
    }

    public void SetMaterial(Material material)
    {
        CompleteTweens(); // Complete fade tweens before switching material
        this.material = material;
    }

    public override void OnDestroy()
    {
        // Destroy the cloned material
        Object.Destroy(material);
    }

    public override Tween DOColor(Color endValue, float duration) => material.DOColor(endValue, duration);
}
