using UnityEngine;
using System.Collections.Generic;

public class ResourceLoader : SingletonMonoBehaviour<ResourceLoader>
{
    public ParticleSystem LoadVFX(VFXType type, Transform parent = null) => Util.Instantiate(prefabVFXs[type], parent);
    private Dictionary<VFXType, ParticleSystem> prefabVFXs;

    public AudioSource LoadSnd(SNDType type, Transform parent = null) => Util.Instantiate(prefabSNDs[type], parent);
    private Dictionary<SNDType, AudioSource> prefabSNDs;

    public EnemyData enemyData { get; private set; }
    public EnemyTypesData enemyTypesData { get; private set; }

    public ItemData itemData { get; private set; }
    public ItemTypesData itemTypesData { get; private set; }

    public FloorMaterialsData floorMaterialsData { get; private set; }
    public FloorMessagesData floorMessagesData { get; private set; }

    public FaceClipsSet faceClipsSet { get; private set; }

    private YenBagData yenBagData;
    public YenBagSource YenBagSource(BagSize size) => yenBagData.Param((int)size);

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
            { VFXType.PitDrop,      Resources.Load<ParticleSystem>("Prefabs/Effect/FX_PIT_DROP")        },
        };

        prefabSNDs = new Dictionary<SNDType, AudioSource>()
        {
            { SNDType.Teleport,             Resources.Load<AudioSource>("Prefabs/Sound/SND_TELEPORT")           },
            { SNDType.TeleportDest,         Resources.Load<AudioSource>("Prefabs/Sound/SND_TELEPORT_DEST")      },
            { SNDType.ResurrectionSkull,    Resources.Load<AudioSource>("Prefabs/Sound/SND_RESURRECTION_SKULL") },
            { SNDType.PitDrop,              Resources.Load<AudioSource>("Prefabs/Sound/SND_PIT_DROP")           },
        };

        enemyData = Resources.Load<EnemyData>("DataAssets/Character/EnemyData");
        enemyTypesData = Resources.Load<EnemyTypesData>("DataAssets/Map/EnemyTypesData");

        itemData = Resources.Load<ItemData>("DataAssets/Item/ItemData");
        itemTypesData = Resources.Load<ItemTypesData>("DataAssets/Map/ItemTypesData");

        floorMaterialsData = Resources.Load<FloorMaterialsData>("DataAssets/Map/FloorMaterialsData");
        floorMessagesData = Resources.Load<FloorMessagesData>("DataAssets/Message/FloorMessagesData");

        faceClipsSet = Resources.Load<FaceClipsData>("DataAssets/Character/FaceClipsData").Param(0);

        yenBagData = Resources.Load<YenBagData>("DataAssets/Result/YenBagData");
    }
}
