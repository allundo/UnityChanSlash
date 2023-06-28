using UnityEngine;
public class GameManagerTest : GameManager
{
    protected override void Awake()
    {
        resourceFX = new ResourceFX(transform);
    }

    protected override void Start()
    {
        // Dummy object
    }
}