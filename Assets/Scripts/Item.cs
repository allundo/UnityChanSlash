using UnityEngine;
using DG.Tweening;

public class Item : MonoBehaviour
{
    public enum ItemTypeEnum
    {
        Wood,
        Stone,
        ThrowAxe
    }

    [SerializeField] private ItemTypeEnum type = default;

    public void Initialize()
    {
        Collider cld = GetComponent<Collider>();
        cld.enabled = false;

        Transform tf = transform;
        Vector3 dropPosition = transform.localPosition + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        tf.DOLocalMove(dropPosition, 0.5f).Play();
        Vector3 defaultScale = tf.localScale;
        tf.localScale = Vector3.zero;
        tf.DOScale(defaultScale, 0.5f)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => { cld.enabled = true; })
            .Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        OwnedItemsData.Instance.Add(type);
        OwnedItemsData.Instance.Save();

        foreach (OwnedItem item in OwnedItemsData.Instance.OwnedItems)
        {
            Debug.Log(item.Type + " を " + item.Number + "個所持");
        }

        Destroy(gameObject);
    }
}
