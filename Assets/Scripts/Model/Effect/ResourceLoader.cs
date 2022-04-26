using UnityEngine;
using System.Collections.Generic;

public class ResourceLoader : SingletonMonoBehaviour<ResourceLoader>
{
    public ParticleSystem LoadVFX(VFXType type) => Util.Instantiate(prefabVFXs[type]);
    private Dictionary<VFXType, ParticleSystem> prefabVFXs;

    public AudioSource LoadSnd(SNDType type) => Util.Instantiate(prefabSNDs[type]);
    private Dictionary<SNDType, AudioSource> prefabSNDs;

    protected override void Awake()
    {
        base.Awake();

        prefabVFXs = new Dictionary<VFXType, ParticleSystem>()
        {
            { VFXType.Iced,         Resources.Load<ParticleSystem>("Prefabs/Effect/FX_ICED")            },
            { VFXType.IceCrash,     Resources.Load<ParticleSystem>("Prefabs/Effect/FX_ICE_CRASH")       },
            { VFXType.Teleport,     Resources.Load<ParticleSystem>("Prefabs/Effect/FX_TELEPORT")        },
            { VFXType.TeleportDest, Resources.Load<ParticleSystem>("Prefabs/Effect/FX_TELEPORT_DEST")   },
            { VFXType.Resurrection, Resources.Load<ParticleSystem>("Prefabs/Effect/FX_RESURRECTION")    },
        };

        prefabSNDs = new Dictionary<SNDType, AudioSource>()
        {
            { SNDType.Teleport,             Resources.Load<AudioSource>("Prefabs/Sound/SND_TELEPORT")          },
            { SNDType.TeleportDest,         Resources.Load<AudioSource>("Prefabs/Sound/SND_TELEPORT_DEST")     },
            { SNDType.ResurrectionSkull,    Resources.Load<AudioSource>("Prefabs/Sound/SND_RESURRECTION_SKULL") },
        };
    }
}