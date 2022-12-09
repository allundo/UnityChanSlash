using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GroundCoin : MonoBehaviour
{
    private static Dictionary<GroundCoin, CombineInstance> coins = new Dictionary<GroundCoin, CombineInstance>();
    private static List<Mesh> combinedMeshes = new List<Mesh>();
    private static List<GameObject> combinedCoins = new List<GameObject>();

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
            StoreMesh();
            Disable();
        }
    }

    private void Disable()
    {
        body.Sleep();
        body.useGravity = false;
        col.enabled = false;
        enabled = false;
    }

    private void Init(bool isActive = false)
    {
        body.WakeUp();
        body.useGravity = true;
        col.enabled = true;
        enabled = true;
        gameObject.SetActive(isActive);
    }

    private void StoreMesh()
    {
        coins[this] = new CombineInstance() { mesh = GetComponent<MeshFilter>().sharedMesh, transform = transform.localToWorldMatrix };

        if (coins.Count == 20)
        {
            Mesh combinedMesh = new Mesh();
            name = combinedMesh.name = "500YenCoins";
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            combinedMesh.CombineMeshes(coins.Select(kv => kv.Value).ToArray(), true);

            transform.localScale = Vector3.one;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            GetComponent<MeshFilter>().mesh = combinedMesh;
            combinedMeshes.Add(combinedMesh);
            combinedCoins.Add(gameObject);

            coins.Remove(this);
            coins.ForEach(kv => kv.Key.Init());
            coins.Clear();

        }
    }

    public static void Release()
    {
        coins.Clear();
        combinedMeshes.ForEach(mesh => Destroy(mesh));
        combinedCoins.ForEach(obj => Destroy(obj));
    }
}
