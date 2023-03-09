using UnityEngine;

public class MaterialAlphaSetter : ImageAlphaSetter
{
    protected Material material;
    public override Color color
    {
        get { return image.material.color; }
        set { image.material.color = value; }
    }

    protected override void Awake()
    {
        base.Awake();
        material = new Material(image.material);
        image.material = material;
    }
}
