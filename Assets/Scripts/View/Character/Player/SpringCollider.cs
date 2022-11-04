using UnityEngine;

public class SpringCollider : MonoBehaviour
{
    //半径
    public float radius = 0.5f;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
