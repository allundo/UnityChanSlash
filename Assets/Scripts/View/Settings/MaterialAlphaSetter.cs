using UnityEngine;

public class MaterialAlphaSetter : ImageAlphaSetter
{
    protected Material material;
    public override Color color
    {
        get { return image.material.color; }
        set { image.material.color = value; }
    }
}
