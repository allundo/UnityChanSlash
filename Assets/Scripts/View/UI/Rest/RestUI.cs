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
    private FadeTween fade;

    public bool isActive { get; protected set; } = false;

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData) => Inactivate();

    protected void Awake()
    {
        image = GetComponent<Image>();
        fade = new FadeTween(image);
    }

    protected void Start()
    {
        restButton.onClick.AddListener(Activate);
        resumeButton.onPush.AddListener(Inactivate);

        Inactivate();
    }

    public void Activate()
    {
        isActive = true;
        restButton.interactable = false;
        image.raycastTarget = true;
        restLifeGauge.gameObject.SetActive(true);
        txtRest.gameObject.SetActive(true);
        cover.FadeOut(0.01f).Play();
        resumeButton.Show().Play();
        GameManager.Instance.TimeScale();
    }

    public void Inactivate()
    {
        isActive = false;
        restButton.interactable = true;
        image.raycastTarget = false;
        restLifeGauge.gameObject.SetActive(false);
        txtRest.gameObject.SetActive(false);
        cover.FadeIn(0.01f).Play();
        resumeButton.Hide().Play();
        GameManager.Instance.Resume();
    }

    public void OnLifeChange(float life, float lifeMax)
        => restLifeGauge.OnLifeChange(life, lifeMax);

    public void OnDamage()
    {
        Inactivate();

        DOTween.Sequence()
            .AppendCallback(() => fade.color = Color.white)
            .AppendCallback(() => fade.SetAlpha(1f))
            .AppendInterval(0.025f)
            .AppendCallback(() => fade.color = Color.red)
            .Append(fade.Out(0.8f, 0f, null, null, false))
            .SetUpdate(true)
            .Play();
    }
}
