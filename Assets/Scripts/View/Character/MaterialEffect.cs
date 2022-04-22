using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface IMaterialEffect
{
    List<Material> CopyMaterials();
    void KillAllTweens();
    void InitEffects();
}

public abstract class MaterialEffect : IMaterialEffect
{
    protected List<Material> materials;

    protected int propID;
    protected abstract string propName { get; }
    protected abstract void InitProperty(Material mat, int propID);

    public MaterialEffect(Transform targetTf)
    {
        RetrieveMaterials(targetTf);
    }

    /// <summary>
    /// Retrieve and store materials having the property specified by propName
    /// </summary>
    /// <returns></returns>
    public void RetrieveMaterials(Transform targetTf)
    {
        propID = Shader.PropertyToID(propName);
        materials = new List<Material>();

        foreach (Renderer renderer in targetTf.GetComponentsInChildren<Renderer>())
        {
            materials.AddRange(renderer.materials.Where(mat => mat.HasProperty(propID)));
        }
    }

    public MaterialEffect(List<Material> materials)
    {
        propID = Shader.PropertyToID(propName);
        this.materials = materials;
    }

    public List<Material> CopyMaterials() => new List<Material>(materials);

    public virtual void InitEffects()
    {
        materials.ForEach(mat => InitProperty(mat, propID));
    }

    /// <summary>
    /// Release all playing tweens. Mainly called just before destroy.
    /// </summary>
    public abstract void KillAllTweens();

}