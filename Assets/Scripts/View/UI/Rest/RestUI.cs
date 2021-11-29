using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RestUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button restButton = default;
    [SerializeField] private ResumeButton resumeButton = default;
    [SerializeField] private RestLifeGauge restLifeGauge = default;
    [SerializeField] private TextMeshProUGUI txtRest = default;
    [SerializeField] private CoverScreen cover = default;

    private Image image;

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData) => Inactivate();

    protected void Awake()
    {
        image = GetComponent<Image>();
    }

    protected void Start()
    {
        restButton.onClick.AddListener(Activate);
        resumeButton.onPush.AddListener(Inactivate);

        Inactivate();
    }

    public void Activate()
    {
        restButton.interactable = false;
        image.raycastTarget = true;
        restLifeGauge.gameObject.SetActive(true);
        txtRest.gameObject.SetActive(true);
        cover.FadeOut(0.01f).Play();
        resumeButton.Show().Play();
    }

    public void Inactivate()
    {
        restButton.interactable = true;
        image.raycastTarget = false;
        restLifeGauge.gameObject.SetActive(false);
        txtRest.gameObject.SetActive(false);
        cover.FadeIn(0.01f).Play();
        resumeButton.Hide().Play();
    }
}