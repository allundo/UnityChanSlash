using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ItemButton : MonoBehaviour
{
    [SerializeField] private ItemTypeStripeMap[] itemSprites = default;
    [SerializeField] private Image image = default;
    [SerializeField] private Text number = default;
    private Button _button;
    private OwnedItem _ownedItem;

    public OwnedItem OwnedItem
    {
        get { return _ownedItem; }
        set
        {
            _ownedItem = value;

            bool isEmpty = null == _ownedItem;
            image.gameObject.SetActive(!isEmpty);
            number.gameObject.SetActive(!isEmpty);
            _button.interactable = !isEmpty;
            if (!isEmpty)
            {
                image.sprite = itemSprites.First(item => item.type == _ownedItem.Type).sprite;
                number.text = "" + _ownedItem.Number;
            }

        }

    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // TODO ボタン押下時の動作
    }

    [Serializable]
    public class ItemTypeStripeMap
    {
        public Item.ItemTypeEnum type;
        public Sprite sprite;
    }
}
