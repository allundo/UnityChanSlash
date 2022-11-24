using UnityEngine;

public class SpringBone : MonoBehaviour
{
    /// <summary>
    /// Child bone
    /// </summary>
    [SerializeField] private Transform child;

    [SerializeField] private Vector3 boneAxis = new Vector3(-1.0f, 0.0f, 0.0f);

    /// <summary>
    /// Tip radius to use collision.
    /// </summary>
    [SerializeField] private float radius = 0.05f;

    /// <summary>
    /// Spring elastic force that restore to original angle.
    /// </summary>
    [SerializeField] private float stiffnessForce = 0.01f;

    /// <summary>
    /// Dampen spring force
    /// </summary>
    [SerializeField] private float dragForce = 0.4f;

    [SerializeField] private SpringCollider[] colliders;

    private float springLength;
    private Quaternion defaultLocalRotation;
    private Vector3 currTipPos;
    private Vector3 springForce;

    private Vector3 BoneVector => currTipPos - transform.position;

    private void Awake()
    {
        // Store local rotations on Awake() because Test Runner overwrites them before Start().
        defaultLocalRotation = transform.localRotation;
    }

    private void Start()
    {
        springLength = Vector3.Distance(transform.position, child.position);
        currTipPos = child.position;
    }

    public void RestoreBoneLength()
    {
        currTipPos = transform.position + (BoneVector.normalized * springLength);
    }

    public void UpdateSpring(Vector3 windForce)
    {
        transform.localRotation = defaultLocalRotation;

        // Store previous spring bone tip position
        Vector3 prevTipPos = currTipPos;

        // Apply stored spring force with stiffness
        currTipPos += springForce + transform.rotation * (boneAxis * stiffnessForce);
        RestoreBoneLength();

        // Collision with spring colliders
        colliders.ForEach(collider =>
        {
            float distance = radius + collider.radius;
            float sqrDistance = distance * distance;
            Vector3 tipToCol = collider.transform.position - currTipPos;
            if (tipToCol.sqrMagnitude < sqrDistance)
            {
                currTipPos = collider.transform.position - tipToCol.normalized * distance;
                RestoreBoneLength();
            }
        });

        // Apply rotation to the bone
        Vector3 aimVector = transform.TransformDirection(boneAxis);
        Quaternion aimRotation = Quaternion.FromToRotation(aimVector, BoneVector);
        transform.rotation = aimRotation * transform.rotation;

        // Calculate next spring force
        springForce = (currTipPos - prevTipPos) * (1f - dragForce) + windForce;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currTipPos, radius);
    }
#endif
}
