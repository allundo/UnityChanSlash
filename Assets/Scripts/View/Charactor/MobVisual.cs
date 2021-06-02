using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MobVisual : MonoBehaviour
{
    protected List<Material> flashMaterials = new List<Material>();

    protected virtual void Awake()
    {
        StoreMaterialColors();
    }

    protected void StoreMaterialColors()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_AdditiveColor"))
                {
                    flashMaterials.Add(mat);
                }
            }
        }
    }

    public void DamageFlash(float damage, float lifeMax)
    {
        if (damage < 0.0001f) return;

        float rate = Mathf.Clamp(damage / lifeMax, 0.01f, 1.0f);

        foreach (Material mat in flashMaterials)
        {
            Sequence flash = DOTween.Sequence().Append(mat.DOColor(Color.white, 0.02f));

            if (rate > 0.1f)
            {
                flash.Append(mat.DOColor(Color.black, 0.02f));
                flash.Append(mat.DOColor(Color.red, 0.02f));
            }

            flash.Append(mat.DOColor(Color.black, 2.0f * rate)).Play();
        }
    }
}
