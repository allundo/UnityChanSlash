using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MiniMapHandler : MonoBehaviour
{
    [SerializeField] private MiniMap miniMap = default;

    public void SwitchWorldMap(WorldMap map)
    {
        miniMap.SwitchWorldMap(map);
        gameObject.SetActive(false);
    }

    public void OnStartFloor()
    {
        gameObject.SetActive(true);
        miniMap.UpdateMiniMap();
    }

    public void UpdateMiniMap() => miniMap.UpdateMiniMap();
    public PlayerSymbol Turn(IDirection dir) => miniMap.Turn(dir);

    private void OnTriggerEnter(Collider col)
    {
        miniMap.OnEnemyFind(col);
    }

    private void OnTriggerExit(Collider col)
    {
        miniMap.OnEnemyLeft(col);
    }
}
