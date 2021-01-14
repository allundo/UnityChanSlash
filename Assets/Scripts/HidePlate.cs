using UnityEngine;
using DG.Tweening;

public class HidePlate : MonoBehaviour
{
    public Plate plate = Plate.NONE;
    public bool IsActive { get; private set; } = true;
    public HidePlate FadeIn(float duration = 0.01f)
    {
        IsActive = true;

        GetComponent<Renderer>().material
            .DOFade(1.0f, duration)
            .SetEase(Ease.Linear)
            .Play();

        return this;
    }

    public void Remove(float duration = 0.01f)
    {
        GetComponent<Renderer>().material
            .DOFade(0.0f, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                IsActive = false;
                transform.position = new Vector3(-100.0f, 0.0f, -100.0f);
            })
            .Play();
    }

    public static HidePlate GetInstance(GameObject go, Vector3 pos, Quaternion rotation, Transform parent, Plate plate)
    {
        HidePlate instance = Instantiate(go, pos, rotation, parent).GetComponent<HidePlate>();
        instance.plate = plate;
        return instance;
    }
}
