using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class LogoAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform rtBall = default;
    [SerializeField] private RectTransform rtFrame = default;
    [SerializeField] private Button startButton = default;
    [SerializeField] private Sprite frameEmpty = default;
    [SerializeField] private ParticleSystem vfxRain = default;
    [SerializeField] private Camera mainCamera = default;
    private Image imageFrame;

    private Vector2 defaultSize;

    private RectTransform rtButton;
    private RectTransform rtBackGround;

    private AsyncOperation asyncLoad;

    void Awake()
    {
        startButton.onClick.AddListener(SceneTransition);

        startButton.interactable = false;
        rtButton = startButton.GetComponent<RectTransform>();
        rtBackGround = GetComponent<RectTransform>();

        imageFrame = rtFrame.GetComponent<Image>();
        imageFrame.fillAmount = 0f;

        rtBall.anchoredPosition = new Vector2(-1080.0f, 0f);

        rtButton.anchoredPosition = new Vector2(0f, 1920.0f);
        defaultSize = rtButton.sizeDelta;

        mainCamera.transform.position = new Vector3(-10f, 1f, 2.5f);
    }

    void Start()
    {
        DOTween.Sequence()
            .Append(rtBall.DOAnchorPos(Vector2.zero, 1f).SetEase(Ease.InQuad))
            .Join(rtBall.DORotate(new Vector3(0f, 0f, -360f), 1f, RotateMode.FastBeyond360).SetEase(Ease.InQuad))
            .Append(rtBall.DOAnchorPos(new Vector2(10f, 0f), 0.2f).SetEase(Ease.OutQuad))
            .Join(rtBall.DORotate(new Vector3(0f, 0f, -10f), 0.2f, RotateMode.Fast).SetEase(Ease.OutQuad))
            .Append(rtBall.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.InQuad))
            .Join(rtBall.DORotate(Vector3.zero, 0.2f, RotateMode.Fast).SetEase(Ease.InQuad))
            .AppendInterval(0.4f)
            .Append(rtBall.DOAnchorPos(new Vector2(0f, -50f), 0.1f).SetEase(Ease.OutQuad))
            .Append(rtBall.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.InQuad))
            .AppendInterval(0.1f)
            .Append(DOTween.To(() => imageFrame.fillAmount, fill => imageFrame.fillAmount = fill, 1f, 0.15f).SetEase(Ease.Linear))
            .AppendInterval(0.15f)
            .AppendCallback(() => StartCoroutine(LoadScene()))
            .Play();

        DOTween.Sequence()
            .AppendInterval(0.8f)
            .Append(rtButton.DOAnchorPos(Vector2.zero, 1.0f).SetEase(Ease.InQuad))
            .Append(rtButton.DOAnchorPos(new Vector2(0f, -100f), 0.1f).SetEase(Ease.OutQuad))
            .Join(rtButton.DOSizeDelta(new Vector2(defaultSize.x * 1.5f, defaultSize.y * 0.5f), 0.1f).SetEase(Ease.OutQuad))
            .Append(rtButton.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.InQuad))
            .Join(rtButton.DOSizeDelta(defaultSize, 0.1f).SetEase(Ease.InQuad))
            .Play();
    }

    private IEnumerator LoadScene()
    {
        asyncLoad = SceneManager.LoadSceneAsync(1);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return new WaitForSeconds(3);
        }

        ToTitle();
    }

    private void ToTitle()
    {
        imageFrame.sprite = frameEmpty;

        vfxRain?.Stop();

        rtBackGround
            .DOAnchorPos(new Vector2(2820f, 0), 1.8f)
            .SetDelay(0.2f)
            .OnComplete(() => rtBackGround.transform.gameObject.SetActive(false))
            .Play();

        mainCamera.transform.DOMove(new Vector3(0f, 1f, 2.5f), 1.2f).Play();

        DOTween.Sequence()
            .Append(rtButton.DOAnchorPos(new Vector2(0f, -100f), 0.1f).SetEase(Ease.OutQuad))
            .Join(rtButton.DOSizeDelta(new Vector2(defaultSize.x * 1.5f, defaultSize.y * 0.5f), 0.1f).SetEase(Ease.OutQuad))
            .Append(rtButton.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.InQuad))
            .Join(rtButton.DOSizeDelta(defaultSize, 0.1f).SetEase(Ease.InQuad))
            .Append(rtButton.DOJump(new Vector3(-240f, 0f, 0f), 1000f, 1, 0.6f).SetRelative())
            .Join(rtButton.DOSizeDelta(defaultSize * 0.5f, 0.1f).SetEase(Ease.Linear))
            .Join(rtButton.DORotate(new Vector3(0f, 0f, 720f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear))
            .AppendCallback(() => startButton.interactable = true)
            .Play();
    }

    private void SceneTransition()
    {
        asyncLoad.allowSceneActivation = true;
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
        SceneManager.UnloadSceneAsync(0);
    }


}
