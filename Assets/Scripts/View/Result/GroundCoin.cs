using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GroundCoin : MonoBehaviour
{
    private static Dictionary<GameObject, CombineInstance> coins = new Dictionary<GameObject, CombineInstance>();
    private static Mesh mesh500Yen = null;
    private static List<Mesh> combinedMeshes = new List<Mesh>();

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
            StoreMesh(gameObject);
            Destroy(col);
            Destroy(body);
            Destroy(this);
        }
    }

    private static void StoreMesh(GameObject coin)
    {
        if (mesh500Yen == null) mesh500Yen = coin.GetComponent<MeshFilter>().mesh;

        coins[coin] = new CombineInstance() { mesh = mesh500Yen, transform = coin.transform.localToWorldMatrix };

        if (coins.Count == 20)
        {
            Mesh combinedMesh = new Mesh();
            coin.name = combinedMesh.name = "500YenCoins";
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            combinedMesh.CombineMeshes(coins.Select(kv => kv.Value).ToArray(), true);

            coin.transform.localScale = Vector3.one;
            coin.transform.position = Vector3.zero;
            coin.transform.rotation = Quaternion.identity;
            coin.GetComponent<MeshFilter>().mesh = combinedMesh;
            combinedMeshes.Add(combinedMesh);

            coins.Remove(coin);
            coins.ForEach(kv => Destroy(kv.Key));
            coins.Clear();
        }
    }

    public static void Release()
    {
        coins.Clear();
        Destroy(mesh500Yen);
        combinedMeshes.ForEach(mesh => Destroy(mesh));
    }
}
