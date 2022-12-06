using UnityEngine;
using UniRx;

[RequireComponent(typeof(CapsuleCollider))]
public class CapsuleCoin : MonoBehaviour
{
    [SerializeField] private Rigidbody prefabGroundCoin = default;
    private GameObject ground;

    private Rigidbody body;
    private CapsuleCollider col;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    public void SetGround(GameObject ground)
    {
        this.ground = ground;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != ground) return;

        Observable.NextFrame().Subscribe(_ =>
        {
            var coin = Object.Instantiate(prefabGroundCoin, transform.position, transform.rotation);

            coin.velocity = body.velocity;
            coin.angularVelocity = body.angularVelocity;

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
