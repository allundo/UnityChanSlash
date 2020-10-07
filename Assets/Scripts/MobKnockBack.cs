using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MobStatus))]
public class MobKnockBack : MonoBehaviour
{
    [SerializeField] private float damageCooldown = 0.5f;
    private MobStatus status;

    private void Start()
    {
        status = GetComponent<MobStatus>();
    }

    public void OnDamage()
    {
        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(damageCooldown);
        status.GoToNormalStateIfPossible();
    }
}