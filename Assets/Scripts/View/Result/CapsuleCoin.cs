using UnityEngine;
using UniRx;

[RequireComponent(typeof(CapsuleCollider))]
public class CapsuleCoin : MonoBehaviour
{
    private GroundCoinGenerator generator;

    private Rigidbody body;
    private CapsuleCollider col;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    public void SetGenerator(GroundCoinGenerator generator)
    {
        this.generator = generator;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != generator.Ground) return;

        Observable.NextFrame().Subscribe(_ =>
        {
            var coin = generator.Spawn(body);

            col.enabled = false;
            gameObject.SetActive(false);
        }).AddTo(this);
    }

    public void Activate()
    {
        Vector2 circle = UnityEngine.Random.insideUnitCircle;
        transform.position = new Vector3(circle.x, 5f, circle.y);

        gameObject.SetActive(true);

        // Set enabling delay not to collide with cloth on initializing position.
        Observable.NextFrame().Subscribe(_ => col.enabled = true).AddTo(this);
    }
}
