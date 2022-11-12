using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceLoader : SingletonMonoBehaviour<ResourceLoader>
{
    public ParticleSystem LoadVFX(VFXType type, Transform parent = null) => Util.Instantiate(prefabVFXs[type], parent);
    private Dictionary<VFXType, ParticleSystem> prefabVFXs;

    public AudioSource LoadSnd(SNDType type, Transform parent = null) => Util.Instantiate(prefabSNDs[type], parent);
    private Dictionary<SNDType, AudioSource> prefabSNDs;

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
    private Dictionary<ItemType, EquipmentType> ItemEquipmentMap
        = new Dictionary<ItemType, EquipmentType>()
        {
            { ItemType.Null,        EquipmentType.BareHand  },
            { ItemType.LongSword,   EquipmentType.LongSword },
            { ItemType.Katana,      EquipmentType.Katana    },
            { ItemType.KeyBlade,    EquipmentType.KeyBlade  },
        };
    public EquipmentSource GetEquipmentSource(ItemType type)
    {
        EquipmentType equipmentType;
        if (ItemEquipmentMap.TryGetValue(type, out equipmentType))
        {
            return equipmentData.Param((int)equipmentType);
        }
        return null;
    }

    public EquipmentSource GetEquipmentOrDefault(ItemType type)
    {
        EquipmentType equipmentType;
        if (!ItemEquipmentMap.TryGetValue(type, out equipmentType))
        {
            equipmentType = EquipmentType.BareHand;
        }
        return equipmentData.Param((int)equipmentType);
    }

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
        };

        enemyData = Resources.Load<EnemyData>("DataAssets/Character/EnemyData");
        enemyTypesData = Resources.Load<EnemyTypesData>("DataAssets/Map/EnemyTypesData");
        enemyCauseData = Resources.Load<EnemyCauseData>("DataAssets/Character/EnemyCauseData");

        itemData = Resources.Load<ItemData>("DataAssets/Item/ItemData");
        itemTypesData = Resources.Load<ItemTypesData>("DataAssets/Map/ItemTypesData");
        itemInfo = new ItemInfoLoader(itemData).LoadItemInfo();

        equipmentData = Resources.Load<EquipmentData>("DataAssets/Item/EquipmentData");

        floorMaterialsData = Resources.Load<FloorMaterialsData>("DataAssets/Map/FloorMaterialsData");
        floorMessagesData = Resources.Load<FloorMessagesData>("DataAssets/Message/FloorMessagesData");

        faceClipsSet = Resources.Load<FaceClipsData>("DataAssets/Character/FaceClipsData").Param(0);

        animationCurveData = Resources.Load<AnimationCurveData>("DataAssets/System/AnimationCurveData");

        yenBagData = Resources.Load<YenBagData>("DataAssets/Result/YenBagData");
    }
}
