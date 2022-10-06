using UnityEngine;

public class InputRegion : MonoBehaviour
{
    [SerializeField] private AttackButtonRegion[] attackButtonRegions = default;

    public AttackButtonRegion AttackButtonRegion(int index) => attackButtonRegions[index];
}