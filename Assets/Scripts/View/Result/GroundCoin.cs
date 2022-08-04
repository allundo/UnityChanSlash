using UnityEngine;
public class GroundCoin : MonoBehaviour
{
    private MeshCollider col;
    private Rigidbody body;
    private float force;

    void Awake()
    {
        col = GetComponent<MeshCollider>();
        body = GetComponent<Rigidbody>();
        force = 0f;
    }

    void Update()
    {
        // Lay down standing coins with random force
        body.AddForce(Random.onUnitSphere * (force += 0.01f));

        if (body.velocity.sqrMagnitude < 0.001f && body.angularVelocity.sqrMagnitude < 0.01f)
        {
            Destroy(col);
            Destroy(body);
            Destroy(this);
        }
    }
}
