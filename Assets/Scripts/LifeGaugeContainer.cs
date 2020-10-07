using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LifeGaugeContainer : MonoBehaviour
{
    public static LifeGaugeContainer Instance
    {
        get { return _instance; }
    }

    private static LifeGaugeContainer _instance;
    private RectTransform rectTransform;
    private Dictionary<MobStatus, LifeGauge> _statusLifeBarMap = new Dictionary<MobStatus, LifeGauge>();
    [SerializeField] private Camera mainCamera = default;
    [SerializeField] private LifeGauge lifeGaugePrefab = default;

    // Start is called before the first frame update
    private void Awake()
    {
        if (null != _instance)
        {
            throw new System.Exception("LifeBarContainer instance already exists");
        }
        _instance = this;

        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    public void Add(MobStatus status)
    {
        LifeGauge lifeGauge = Instantiate(lifeGaugePrefab, transform);
        lifeGauge.Initialize(rectTransform, mainCamera, status);
        _statusLifeBarMap.Add(status, lifeGauge);
    }
    public void Remove(MobStatus status)
    {
        Destroy(_statusLifeBarMap[status].gameObject);
        _statusLifeBarMap.Remove(status);
    }
}
