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

        if (coins.Count == 16)
        {
            Mesh combinedMesh = new Mesh();
            name = combinedMesh.name = "500YenCoins";
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            combinedMesh.CombineMeshes(coins.Select(kv => kv.Value).ToArray(), true);

            transform.localScale = Vector3.one;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            GetComponent<MeshFilter>().sharedMesh = combinedMesh;
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
        int unit = numOfMeshes > 9 ? numOfMeshes / 5 : numOfMeshes;

        for (int combineCount = numOfMeshes; combineCount > 0; combineCount = combinedMeshes.Count())
        {
            if (combineCount > unit) combineCount = unit;

            GameObject[] objToCombine = combinedCoins.Take(combineCount).ToArray();

            // Reuse last index of GameObjects to combine meshes
            MeshFilter wholeMeshObj = objToCombine[combineCount - 1].GetComponent<MeshFilter>();

            CombineInstance[] combineInstances = combinedMeshes
                .Take(combineCount)
                .Select(m => new CombineInstance() { mesh = m, transform = wholeMeshObj.transform.localToWorldMatrix })
                .ToArray();

            Mesh wholeMesh = new Mesh();
            wholeMesh.name = wholeMeshObj.name = "Combined500YenCoins";
            wholeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            wholeMesh.CombineMeshes(combineInstances, true);

            wholeMeshObj.sharedMesh = wholeMesh;

            fullCombinedMeshes.Add(wholeMeshObj);

            combinedMeshes.RemoveRange(0, combineCount);

            objToCombine.Take(combineCount - 1).ForEach(obj => Destroy(obj));
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
