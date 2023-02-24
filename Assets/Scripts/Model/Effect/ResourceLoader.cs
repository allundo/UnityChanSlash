using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceLoader : SingletonMonoBehaviour<ResourceLoader>
{
    public ParticleSystem LoadVFX(VFXType type, Transform parent = null) => Util.Instantiate(prefabVFXs[type], parent);
    private Dictionary<VFXType, ParticleSystem> prefabVFXs;

    public AudioSource LoadSnd(SNDType type, Transform parent = null) => Util.Instantiate(prefabSNDs[type], parent);
    private Dictionary<SNDType, AudioSource> prefabSNDs;

    public LevelGainData enemyLevelGainData { get; private set; }

    public EnemyData enemyData { get; private set; }
    public EnemyTypesData enemyTypesData { get; private set; }
    private EnemyCauseData enemyCauseData;
    private Dictionary<AttackType, Func<EnemyCauseSource, string>> EnemyCauseMap
        = new Dictionary<AttackType, Func<EnemyCauseSource, string>>()
        {
            { AttackType.None,      source => source.none   },
            { AttackType.Smash,     source => source.smash  },
            { AttackType.Slash,     source => source.slash  },
            { AttackType.Sting,     source => source.sting  },
            { AttackType.Bite,      source => source.bite   },
            { AttackType.Burn,      source => source.burn   },
            { AttackType.Ice,       source => source.ice    },
            { AttackType.Thunder,   source => source.thunder},
            { AttackType.Light,     source => source.light  },
            { AttackType.Dark,      source => source.dark   },
        };

    public string GetDeadCause(EnemyType enemyType, AttackType attackType)
    {
        var name = enemyData.Param((int)enemyType).name;
        var cause = EnemyCauseMap[attackType](enemyCauseData.Param((int)enemyType));
        return name + cause;
    }

    public ItemData itemData { get; private set; }
    public ItemTypesData itemTypesData { get; private set; }
    public ItemInfo ItemInfo(ItemType type) => itemInfo[type].Generate() as ItemInfo;
    public ItemInfo ItemInfo(ItemType type, int numOfItem) => itemInfo[type].Clone(numOfItem) as ItemInfo;
    private Dictionary<ItemType, ItemInfo> itemInfo;

    private EquipmentData equipmentData;

    private List<ItemType> itemEquipmentList;
    public EquipmentSource GetEquipmentSource(ItemType type)
    {
        int index = itemEquipmentList.IndexOf(type);

        if (index == -1) return null;

        return equipmentData.Param(index);
    }

    public EquipmentSource GetEquipmentOrDefault(ItemType type)
        => GetEquipmentSource(type) ?? equipmentData.Param(0);

    public EquipmentSource GetEquipmentOrDefault(ItemInfo itemInfo)
        => GetEquipmentOrDefault(itemInfo != null ? itemInfo.type : ItemType.Null);

    public EquipmentSource GetEquipmentOrDefault(ItemIcon itemIcon)
        => GetEquipmentOrDefault(itemIcon?.itemInfo);

    public FloorMaterialsData floorMaterialsData { get; private set; }
    public FloorMessagesData floorMessagesData { get; private set; }

    public FaceClipsSet faceClipsSet { get; private set; }

    private AnimationCurveData animationCurveData;
    public AnimationCurve EaseCurve(CurveType type) => animationCurveData.Param((int)type).curve;

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
            { SNDType.DoorOpen,             Resources.Load<AudioSource>("Prefabs/Sound/SND_DOOR_OPEN")          },
            { SNDType.DoorClose,            Resources.Load<AudioSource>("Prefabs/Sound/SND_DOOR_CLOSE")         },
            { SNDType.BoxOpen,              Resources.Load<AudioSource>("Prefabs/Sound/SND_BOX_OPEN")           },
            { SNDType.BoxClose,             Resources.Load<AudioSource>("Prefabs/Sound/SND_BOX_CLOSE")          },
            { SNDType.FloorMove,            Resources.Load<AudioSource>("Prefabs/Sound/SND_FLOOR_MOVE")         },
            { SNDType.Decision,             Resources.Load<AudioSource>("Prefabs/Sound/SND_UI_DECISION")        },
            { SNDType.Select,               Resources.Load<AudioSource>("Prefabs/Sound/SND_UI_SELECT")          },
            { SNDType.DropStart,            Resources.Load<AudioSource>("Prefabs/Sound/SND_TITLE_DROP_START")   },
        };

        enemyLevelGainData = Resources.Load<LevelGainData>("DataAssets/Character/EnemyLevelGainData");

        enemyData = Resources.Load<EnemyData>("DataAssets/Character/EnemyData");
        enemyTypesData = Resources.Load<EnemyTypesData>("DataAssets/Map/EnemyTypesData");
        enemyCauseData = Resources.Load<EnemyCauseData>("DataAssets/Character/EnemyCauseData");

        itemData = Resources.Load<ItemData>("DataAssets/Item/ItemData");
        itemTypesData = Resources.Load<ItemTypesData>("DataAssets/Map/ItemTypesData");
        var itemInfoLoader = new ItemInfoLoader(itemData);
        itemInfo = itemInfoLoader.LoadItemInfo();
        itemEquipmentList = itemInfoLoader.GetEquipmentList();

        equipmentData = Resources.Load<EquipmentData>("DataAssets/Item/EquipmentData");

        floorMaterialsData = Resources.Load<FloorMaterialsData>("DataAssets/Map/FloorMaterialsData");
        floorMessagesData = Resources.Load<FloorMessagesData>("DataAssets/Message/FloorMessagesData");

        faceClipsSet = Resources.Load<FaceClipsData>("DataAssets/Character/FaceClipsData").Param(0);

        animationCurveData = Resources.Load<AnimationCurveData>("DataAssets/System/AnimationCurveData");

        yenBagData = Resources.Load<YenBagData>("DataAssets/Result/YenBagData");
    }
}
