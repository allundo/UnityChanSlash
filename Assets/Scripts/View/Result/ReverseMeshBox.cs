using System.Linq;
using UnityEngine;

public class ReverseMeshBox : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.triangles = mesh.triangles.Reverse().ToArray();
        gameObject.AddComponent<MeshCollider>();
    }

    public void InsertCoin(GameObject coin)
    {
        coin.transform.position = transform.position;
        coin.SetActive(true);
        coin.GetComponent<Rigidbody>().AddForce(Random.insideUnitSphere * 0.01f, ForceMode.Impulse);
    }
}
