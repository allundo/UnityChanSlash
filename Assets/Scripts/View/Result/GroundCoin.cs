using UnityEngine;
public class GroundCoin : MonoBehaviour
{
    private MeshCollider col;
    private Rigidbody body;

    void Awake()
    {
        col = GetComponent<MeshCollider>();
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (body.velocity.sqrMagnitude < 0.005f)
        {
            Destroy(col);
            Destroy(body);
            Destroy(this);
        }
    }

}