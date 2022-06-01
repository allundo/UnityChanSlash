using UnityEngine;
using UniRx;

[RequireComponent(typeof(Collider))]
public class PitControl : MonoBehaviour
{
    protected static ParticleSystem dropVFX = null;
    protected static AudioSource dropSnd = null;

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
        anim.speed = 1f;
        anim.SetTrigger(drop);

        if (isFXActive)
        {
            dropVFX = dropVFX ?? ResourceLoader.Instance.LoadVFX(VFXType.PitDrop);
            dropSnd = dropSnd ?? ResourceLoader.Instance.LoadSnd(SNDType.PitDrop);

            dropVFX.transform.position = dropSnd.transform.position = transform.position;

            dropVFX.Play();
            dropSnd.PlayEx();
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