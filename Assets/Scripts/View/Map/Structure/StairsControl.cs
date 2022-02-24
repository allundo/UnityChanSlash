using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StairsControl : MonoBehaviour
{
    [SerializeField] protected bool isDownStairs = true;
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

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerReactor>() != null)
        {
            GameManager.Instance.EnterStair(isDownStairs);
        }
    }
}
