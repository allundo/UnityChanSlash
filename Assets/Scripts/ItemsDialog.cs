using UnityEngine;

public class ItemsDialog : MonoBehaviour
{
    [SerializeField] private int buttonNumber = 15;
    [SerializeField] private ItemButton itemButton = default;

    private ItemButton[] _itemButtons;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);

        for (int i = 0; i < buttonNumber; i++)
        {
            Instantiate(itemButton, transform);
        }

        _itemButtons = GetComponentsInChildren<ItemButton>();
    }

    // Update is called once per frame
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);

        if (gameObject.activeSelf)
        {
            for (int i = 0; i < buttonNumber; i++)
            {
                _itemButtons[i].OwnedItem = OwnedItemsData.Instance.OwnedItems.Length > i
                    ? OwnedItemsData.Instance.OwnedItems[i]
                    : null;
            }
        }

    }
}
