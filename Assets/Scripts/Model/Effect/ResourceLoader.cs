using UnityEngine;
using System.Collections.Generic;

public class ResourceLoader : SingletonMonoBehaviour<ResourceLoader>
{
    public ParticleSystem LoadVFX(VFXType type) => Util.Instantiate(prefabVFXs[type]);
    private Dictionary<VFXType, ParticleSystem> prefabVFXs;

    public AudioSource LoadSnd(SNDType type) => Util.Instantiate(prefabSNDs[type]);
    private Dictionary<SNDType, AudioSource> prefabSNDs;

    public EnemyData enemyData { get; private set; }
    public EnemyTypesData enemyTypesData { get; private set; }

    public ItemData itemData { get; private set; }
    public ItemTypesData itemTypesData { get; private set; }

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

        enemyData = Resources.Load<EnemyData>("DataAssets/Character/EnemyData");
        enemyTypesData = Resources.Load<EnemyTypesData>("DataAssets/Map/EnemyTypesData");

        itemData = Resources.Load<ItemData>("DataAssets/Item/ItemData");
        itemTypesData = Resources.Load<ItemTypesData>("DataAssets/Map/ItemTypesData");
    }
}
