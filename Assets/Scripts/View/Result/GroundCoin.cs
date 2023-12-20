using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroundCoin : MonoBehaviour
{
    private static Dictionary<GroundCoin, CombineInstance> coins = new Dictionary<GroundCoin, CombineInstance>();
    private static List<Mesh> combinedMeshes = new List<Mesh>();
    private static List<GameObject> combinedCoins = new List<GameObject>();
    private static List<MeshFilter> fullCombinedMeshes = new List<MeshFilter>();

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

        if (coins.Count == 10)
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

    public static IEnumerator FullCombineMeshes()
    {
        var wait1Sec = new WaitForSeconds(1);

        int numOfMeshes = combinedMeshes.Count();
        int unit = numOfMeshes > 5 ? numOfMeshes / 5 : numOfMeshes;

        for (int combineCount = numOfMeshes; combineCount > 0; combineCount = combinedMeshes.Count())
        {
            if (combineCount > unit) combineCount = unit;

            Mesh[] meshesToCombine = combinedMeshes.Take(combineCount).ToArray();

            MeshFilter wholeMeshObj = new GameObject("Combined500YenCoins").AddComponent<MeshFilter>();
            Mesh wholeMesh = new Mesh();
            wholeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            wholeMesh.CombineMeshes(meshesToCombine.Select(m => new CombineInstance() { mesh = m }).ToArray(), true);

            wholeMeshObj.sharedMesh = wholeMesh;

            fullCombinedMeshes.Add(wholeMeshObj);

            meshesToCombine.ForEach(mesh => Destroy(mesh));
            combinedMeshes.RemoveRange(0, combineCount);

            combinedCoins.Take(combineCount).ForEach(obj => Destroy(obj));
            combinedCoins.RemoveRange(0, combineCount);

            yield return wait1Sec;
        }
    }

    public static void Release()
    {
        coins.Clear();
        combinedMeshes.ForEach(mesh => Destroy(mesh));
        combinedCoins.ForEach(obj => Destroy(obj));
        fullCombinedMeshes.ForEach(meshFilter => Destroy(meshFilter.sharedMesh));
    }
}
