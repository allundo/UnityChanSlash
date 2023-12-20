using UnityEngine;
using UniRx;

[RequireComponent(typeof(CapsuleCollider))]
public class CapsuleCoin : MonoBehaviour
{
    private GroundCoinGenerator generator;

    private Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
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
            generator.Spawn(body);
            gameObject.SetActive(false);
        }).AddTo(this);
    }

    public void Activate()
    {
        // Set moving delay not to collide with cloth.
        Observable.NextFrame()
            .ContinueWith(_ =>
            {
                Vector2 circle = UnityEngine.Random.insideUnitCircle;
                body.MovePosition(new Vector3(circle.x, 5f, circle.y));

                // Set enabling delay not to collide with cloth.
                return Observable.NextFrame();
            })
            .Subscribe(_ => gameObject.SetActive(true))
            .AddTo(this);
    }
}
