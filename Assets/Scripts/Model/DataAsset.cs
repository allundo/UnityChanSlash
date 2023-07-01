using UnityEngine;
using System;
using System.Linq;
using TMPro;

public class DataAsset<T> : ScriptableObject
{
    [SerializeField] protected T[] setParams;

    public int Length => setParams.Length;

    public T Param(int index)
    {
        if (index >= setParams.Length) throw new IndexOutOfRangeException("Parameter number " + index + "is not found.");

        return setParams[index];
    }

    public void ForEach(Action<T> action)
    {
        setParams.ForEach(param => action(param));
    }
}

[System.Serializable]
public class Param
{
    [SerializeField] public string name = "キャラクター";

    [SerializeField] public float defaultLifeMax = 10;

    [SerializeField] public float attack = 1.0f;

    [SerializeField] public Status prefab = default;
}

[System.Serializable]
public class MobParam : Param
{
    [SerializeField] public bool isOnGround = true;

    [SerializeField] public float shield = 0.0f;

    [SerializeField] public float magic = 1.0f;

    [SerializeField] public float faceDamageMultiplier = 1.0f;

    [SerializeField] public float sideDamageMultiplier = 1.5f;

    [SerializeField] public float backDamageMultiplier = 2.0f;

    [SerializeField] public float restDamageMultiplier = 6.0f;

    [SerializeField] public float fireDamageMultiplier = 1f;

    [SerializeField] public float iceDamageMultiplier = 1f;

    [SerializeField] public float thunderDamageMultiplier = 1f;

    [SerializeField] public float lightDamageMultiplier = 1f;

    [SerializeField] public float darkDamageMultiplier = 1f;

    [SerializeField] public float armorMultiplier = 1.0f;

    [SerializeField] public float baseExp = 100f;
}

[System.Serializable]
public class EnemyParam : MobParam
{
    [SerializeField] public EnemyType type = EnemyType.None;
    [SerializeField] public Vector3 enemyCore = Vector3.zero;
    [SerializeField] public float tamingProbability = 0f;
    [SerializeField] public EnemyLevelGainType gainType = EnemyLevelGainType.None;
}

[System.Serializable]
public class LevelGain
{
    [SerializeField] public string name = "バランス型";
    [SerializeField] public float lifeMaxGainRatio = 1.0185f;
    [SerializeField] public float attackGain = 2f;
    [SerializeField] public float shieldGain = 2f;
    [SerializeField] public float magicGain = 0.2f;
    [SerializeField] public float armorReduction = 0.02f;
    [SerializeField] public float fireReduction = 0.02f;
    [SerializeField] public float iceReduction = 0.02f;
    [SerializeField] public float thunderReduction = 0.02f;
    [SerializeField] public float lightReduction = 0.02f;
    [SerializeField] public float darkReduction = 0.02f;
}

[System.Serializable]
public class EnemyCauseSource
{
    [SerializeField] public string name = "名前";
    [SerializeField] public string none = default;
    [SerializeField] public string smash = default;
    [SerializeField] public string slash = default;
    [SerializeField] public string sting = default;
    [SerializeField] public string bite = default;
    [SerializeField] public string burn = default;
    [SerializeField] public string ice = default;
    [SerializeField] public string thunder = default;
    [SerializeField] public string light = default;
    [SerializeField] public string dark = default;
}

[System.Serializable]
public class ItemSource
{
    [SerializeField] public string name = "種別";
    [SerializeField] public Material material = default;
    [SerializeField] public Material uiMaterial = default;
    [SerializeField] public ParticleSystem vfx = default;
    [SerializeField] public AudioSource sfx = default;
    [SerializeField] public float duration = 0.2f;
    [SerializeField] private string description = "説明";
    public string Description => description.Replace("\\n", "\n"); // strings on DataAsset cannot include NEW_LINE and escapes \
    [SerializeField] public int unitPrice = 0;
    [SerializeField] public int numOfItemMin = 1;
    [SerializeField] public int numOfItemMax = 1;
}

[System.Serializable]
public class DamageSndSource
{
    [SerializeField] public string name = "攻撃タイプ";
    [SerializeField] public AudioSource damage = default;
    [SerializeField] public AudioSource critical = default;
    [SerializeField] public AudioSource guard = default;
}

[System.Serializable]
public class EnemyTypesSource
{
    [SerializeField] public EnemyType[] types = new EnemyType[0];
}

[System.Serializable]
public class ItemTypesSource
{
    [SerializeField] public ItemType[] randomTypes = new ItemType[0];
    [SerializeField] public ItemType[] fixedTypes = new ItemType[0];
}

[System.Serializable]
public class FloorMaterialsSource
{
    [SerializeField] public string name = "通常ダンジョン";
    [SerializeField] public Material ground = default;
    [SerializeField] public Material wall = default;
    [SerializeField] public Material gate = default;
    [SerializeField] public Material door = default;
    [SerializeField] public Material stairs = default;
    [SerializeField] public Material pitLid = default;
    [SerializeField] public float pitDamage = 1f;
    [SerializeField] public Material hidePlate = default;
    [SerializeField] public Material plateFront = default;
    [SerializeField] public Color pointLightColor = default;
}

[System.Serializable]
public class MessageSource
{
    [SerializeField] public string name = "メッセージ";
    [SerializeField] public string sentence = default;
    [SerializeField] public FaceID face = FaceID.NONE;
    [SerializeField] public float fontSize = 64f;
    [SerializeField] public float literalsPerSec = 20f;
    [SerializeField] public TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;
    [SerializeField] public string title = null;
    [SerializeField] public Sprite spriteImage = null;
    [SerializeField] public Material matImage = null;
    [SerializeField] public string caption = "";
    [SerializeField] public bool ignoreIfRead = false;

    public MessageSource(
        string sentence,
        FaceID face = FaceID.NONE,
        float fontSize = 64f,
        float literalsPerSec = 20f,
        TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft,
        string title = null,
        Sprite spriteImage = null,
        Material matImage = null,
        string caption = null,
        bool ignoreIfRead = false
    )
    {
        this.sentence = sentence;
        this.face = face;
        this.fontSize = fontSize;
        this.literalsPerSec = literalsPerSec;
        this.alignment = alignment;
        this.title = title;
        this.spriteImage = spriteImage;
        this.matImage = matImage;
        this.caption = caption;
        this.ignoreIfRead = ignoreIfRead;
    }

    public MessageSource Convert()
        => new MessageSource(sentence.Replace("\\n", "\n"), face, fontSize, literalsPerSec, alignment, title, spriteImage, matImage, caption, ignoreIfRead);
}

[System.Serializable]
public class SecretMessageSource
{
    [SerializeField] public int secretLevel = 0;
    [SerializeField] public int floor = 0;
    [SerializeField] public bool levelUpIfRead = false;
    [SerializeField] public int alterIfReadNumber = -1;

    [SerializeField] public BoardMessageData data = default;
    [SerializeField] public BoardMessageData alterData = default;
}

[System.Serializable]
public class FloorMessagesSource
{
    [SerializeField] public BoardMessageData[] fixedMessages = new BoardMessageData[0];
    [SerializeField] public BoardMessageData[] randomMessages = new BoardMessageData[0];
    [SerializeField] public BoardMessageData[] bloodMessages = new BoardMessageData[0];
}

[System.Serializable]
public class EndingMessagesSource
{
    [SerializeField] public string name = "時間帯";
    [SerializeField] private string[] messages = default;

    public string[] Messages => messages.Select(str => str.Replace("\\n", "\n")).ToArray();
}

[System.Serializable]
public class FaceClipsSet
{
    [SerializeField] public AnimationClip normal = default;
    [SerializeField] public AnimationClip angry1 = default;
    [SerializeField] public AnimationClip angry2 = default;
    [SerializeField] public AnimationClip eyeClose = default;
    [SerializeField] public AnimationClip smile1 = default;
    [SerializeField] public AnimationClip smile2 = default;
    [SerializeField] public AnimationClip surprise = default;
    [SerializeField] public AnimationClip disattract1 = default;
    [SerializeField] public AnimationClip disattract2 = default;
    [SerializeField] public AnimationClip ashamed = default;
    [SerializeField] public AnimationClip[] mouth = default;
}

[System.Serializable]
public class YenBagSource
{
    [SerializeField] public string name = "サイズ";
    [SerializeField] public float coinScale = 1f;
    [SerializeField] public Vector3 startPosition = default;
    [SerializeField] public Vector3[] rightHandOffsets = default;
    [SerializeField] public Vector3[] catchAngles = default;
    [SerializeField] public float dropDelay = 0.65f;
    [SerializeField] public YenBag prefabYenBag = default;
    [SerializeField] public AudioSource prefabCaughtSnd = default;
    [SerializeField] public AudioSource prefabShakeSnd = default;
}

[System.Serializable]
public class AnimationCurveSource
{
    [SerializeField] public string name = "曲線の説明";
    [SerializeField] public AnimationCurve curve = default;
}

[System.Serializable]
public class EquipmentSource
{
    [SerializeField] public string name = "素手";
    [SerializeField] public float attackGain = 0f;
    [SerializeField] public float shieldGain = 0f;
    [SerializeField] public float armorGain = 0f;
    [SerializeField] public AttackAttr attribute = AttackAttr.None;
    [SerializeField] public Vector3 handRPosition = default;
    [SerializeField] public Vector3 handRRotate = default;
    [SerializeField] public Vector3 handLPosition = default;
    [SerializeField] public Vector3 handLRotate = default;
    [SerializeField] public EquipmentCategory category = default;
    [SerializeField] public GameObject prefabEquipment = default;
}

