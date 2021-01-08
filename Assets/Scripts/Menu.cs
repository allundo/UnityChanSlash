using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private ItemsDialog itemsDialog = default;
    [SerializeField] private Button itemsButton = default;

    private void Start()
    {
        itemsButton.onClick.AddListener(ToggleItemsDialog);
    }
    private void ToggleItemsDialog()
    {
        itemsDialog.Toggle();
    }
}
