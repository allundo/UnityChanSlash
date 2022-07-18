using UnityEngine;
using DG.Tweening;
using TMPro;

public class CaptionController : MonoBehaviour
{
    private string sentence;
    private float currentLiterals = 0;
    private TextMeshProUGUI tm;

    private Tween literalsTween = null;

    void Awake()
    {
        tm = GetComponent<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }

    void Update()
    {
        tm.text = sentence.Substring(0, (int)currentLiterals);
    }

    public void Activate(ActiveMessageData data)
    {
        gameObject.SetActive(true);

        sentence = data.sentence;
        tm.fontSize = data.fontSize;

        currentLiterals = 0f;

        var length = sentence.Length;

        if (data.literalsPerSec > 999.9f)
        {
            currentLiterals = length;
            return;
        }

        literalsTween =
            DOVirtual.Int(0, length, (float)length / data.literalsPerSec, value => currentLiterals = value)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .Play();
    }

    public void Inactivate()
    {
        if (!gameObject.activeSelf) return;

        tm.text = "";
        literalsTween.Kill();
        gameObject.SetActive(false);
    }
}
