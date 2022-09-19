using UnityEngine;

public class InputRegion : MonoBehaviour
{
    [SerializeField] private AttackButtonRegion jabRegion = default;
    [SerializeField] private AttackButtonRegion straightRegion = default;
    [SerializeField] private AttackButtonRegion kickRegion = default;

    public AttackButtonRegion JabRegion => jabRegion;
    public AttackButtonRegion StraightRegion => straightRegion;
    public AttackButtonRegion KickRegion => kickRegion;
}