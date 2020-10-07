using UnityEngine;

[RequireComponent(typeof(MobStatus))]
[RequireComponent(typeof(Item))]
public class MobItemDropper : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float dropRate = 0.1f;
    [SerializeField] private Item itemPrefab = default;
    [SerializeField] private int number = 1;

    private MobStatus _status;
    private bool _isDropInvoked = false;

    // Start is called before the first frame update
    void Start()
    {
        _status = GetComponent<MobStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_status.Life <= 0)
        {
            DropIfNeeded();
        }

    }

    private void DropIfNeeded()
    {
        if (_isDropInvoked) return;

        _isDropInvoked = true;

        if (Random.Range(0.0f, 1.0f) >= dropRate) return;

        for (int i = 0; i < number; i++)
        {
            Item item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            item.Initialize();
        }
    }
}
