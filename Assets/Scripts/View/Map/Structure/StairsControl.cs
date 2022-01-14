using UnityEngine;

public class StairsControl : MonoBehaviour
{
    [SerializeField] protected Renderer rendererWalls = default;
    protected Renderer rendererStairs;

    void Awake()
    {
        rendererStairs = GetComponent<Renderer>();
    }

    public void SetMaterials(Material materialStairs, Material materialWalls = null)
    {
        Util.SwitchMaterial(rendererStairs, materialStairs);
        if (materialWalls != null) Util.SwitchMaterial(rendererWalls, materialWalls);
    }
}
