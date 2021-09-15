using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;

public class ParticleTest : MonoBehaviour
{
    [SerializeField] private UIParticle test = default;
    [SerializeField] private ParticleSystem ps = default;
    // [SerializeField] private UIParticle test2 = default;
    // [SerializeField] private UIParticle test3 = default;
    // [SerializeField] private UIParticle test4 = default;
    // Start is called before the first frame update
    void Start()
    {
        SetUIAsChild(Instantiate(test));
        Instantiate(ps, transform.position, Quaternion.identity, transform);
        // SetUIAsChild(Instantiate(test2));
        // SetUIAsChild(Instantiate(test3));
        // SetUIAsChild(Instantiate(test4));
    }

    private void SetUIAsChild(MaskableGraphic ui)
    {
        ui.transform.SetParent(transform);
        ui.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
