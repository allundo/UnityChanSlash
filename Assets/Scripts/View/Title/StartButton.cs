using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StartButton : MonoBehaviour
{
    private RectTransform rt;
    private Image image;

    // Start is called before the first frame update
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        rt.anchoredPosition = new Vector2(0f, 1920.0f);
    }

    // Update is called once per frame
    void Start()
    {


    }
}
