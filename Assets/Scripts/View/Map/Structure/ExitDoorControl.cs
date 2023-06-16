using UnityEngine;

public class ExitDoorControl : LockedDoorControl
{
    private Material materialExit;
    private Color defaultExitColor;

    protected override void Awake()
    {
        base.Awake();

        materialExit = this.transform.GetChild(3).GetComponent<Renderer>().material;
        defaultExitColor = materialExit.color;
    }

    protected override void SetAlphaToMaterial(float alpha)
    {
        base.SetAlphaToMaterial(alpha);
        materialExit.SetColor("_Color", new Color(defaultExitColor.r, defaultExitColor.g, defaultExitColor.b, alpha));
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerReactor>() != null)
        {
            GameManager.Instance.Exit();
            return;
        }

        base.OnTriggerEnter(other);
    }

    protected override void ForceBreak()
    {
        base.ForceBreak();

        // Enable collider again to wait for player exit.
        doorCollider.enabled = true;
    }

    private void OnDestroy()
    {
        // Destroy cloned material
        Destroy(materialExit);
    }
}
