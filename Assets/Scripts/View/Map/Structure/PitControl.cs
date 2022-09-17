using UnityEngine;
using UniRx;

[RequireComponent(typeof(Collider))]
public class PitControl : MonoBehaviour
{
    [SerializeField] protected Renderer rendererLid = default;
    [SerializeField] protected Renderer rendererPit = default;
    protected PitState pitState;
    protected Animator anim;
    protected int drop;

    void Awake()
    {
        anim = GetComponent<Animator>();
        drop = Animator.StringToHash("Drop");
        anim.speed = 0f;
    }

    protected virtual void Start()
    {
        pitState.Dropped.Subscribe(isFXActive => DropAnim(isFXActive)).AddTo(this);
    }

    public PitControl SetMaterials(Material materialLid, Material materialWalls = null)
    {
        Util.SwitchMaterial(rendererLid, materialLid);
        if (materialWalls != null) Util.SwitchMaterial(rendererPit, materialWalls);
        return this;
    }

    protected void DropAnim(bool isFXActive)
    {
        anim.SetTrigger(drop);

        if (isFXActive)
        {
            anim.speed = 1f;
            GameManager.Instance.PlayVFX(VFXType.PitDrop, transform.position);
            GameManager.Instance.PlaySnd(SNDType.PitDrop, transform.position);
        }
        else
        {
            anim.speed = 100f;
        }
    }

    public PitControl SetState(PitState state)
    {
        this.pitState = state;
        return this;
    }
    public void OnTriggerEnter(Collider other)
    {
        var react = other.GetComponent<PlayerReactor>();
        if (react != null)
        {
            pitState.Drop();
            react.PitFall(pitState.damage);
        }
    }
}