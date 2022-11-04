using UnityEngine;

public class SpringBone : MonoBehaviour
{
    /// <summary>
    /// Child bone
    /// </summary>
    public Transform child;

    public Vector3 boneAxis = new Vector3(-1.0f, 0.0f, 0.0f);

    /// <summary>
    /// Tip radius to use collision.
    /// </summary>
    public float radius = 0.05f;

    /// <summary>
    /// Spring elastic force that restore to original angle.
    /// </summary>
    public float stiffnessForce = 0.01f;

    /// <summary>
    /// Dampen spring force
    /// </summary>
    public float dragForce = 0.4f;

    public Vector3 springForce = new Vector3(0.0f, -0.0001f, 0.0f);
    public SpringCollider[] colliders;

    private float springLength;
    private Quaternion defaultLocalRotation;
    private Vector3 currTipPos;
    private Vector3 prevTipPos;

    private void Awake()
    {
        defaultLocalRotation = transform.localRotation;
    }

    private void Start()
    {
        springLength = Vector3.Distance(transform.position, child.position);
        currTipPos = child.position;
        prevTipPos = child.position;
    }

    private Vector3 DeltaTipPos => currTipPos - prevTipPos;

    public void UpdateSpring()
    {
        //回転をリセット
        transform.localRotation = defaultLocalRotation;

        // Calculate spring force
        Vector3 stiff = transform.rotation * (boneAxis * stiffnessForce);
        Vector3 drag = -DeltaTipPos * dragForce;
        Vector3 force = (stiff + drag + springForce);

        Vector3 deltaTipPosByForce = DeltaTipPos + force;

        prevTipPos = currTipPos;

        // Update spring bone
        currTipPos += deltaTipPosByForce;
        currTipPos = ((currTipPos - transform.position).normalized * springLength) + transform.position;

        //衝突判定
        for (int i = 0; i < colliders.Length; i++)
        {
            if (Vector3.Distance(currTipPos, colliders[i].transform.position) <= (radius + colliders[i].radius))
            {
                Vector3 normal = (currTipPos - colliders[i].transform.position).normalized;
                currTipPos = colliders[i].transform.position + (normal * (radius + colliders[i].radius));
                currTipPos = ((currTipPos - transform.position).normalized * springLength) + transform.position;
            }
        }

        //回転を適用；
        Vector3 aimVector = transform.TransformDirection(boneAxis);
        Quaternion aimRotation = Quaternion.FromToRotation(aimVector, currTipPos - transform.position);
        transform.rotation = aimRotation * transform.rotation;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currTipPos, radius);
    }
#endif
}
