using UnityEngine;
using UnityEngine.UI;

public class AlphaRawImage : MonoBehaviour
{
    [SerializeField] protected float maxAlpha = 1.0f;
    protected RawImage gauge = default;

    void Awake()
    {
        gauge = GetComponent<RawImage>();
    }

    public void SetAlpha(float alpha)
    {
        Color c = gauge.color;
        gauge.color = new Color(c.r, c.g, c.b, alpha * maxAlpha);
    }
}
