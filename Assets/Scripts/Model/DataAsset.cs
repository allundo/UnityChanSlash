using UnityEngine;
using System;
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
}

[System.Serializable]
public class EnemyParam : MobParam
{
    [SerializeField] public EnemyType type = EnemyType.None;
    [SerializeField] public Vector3 enemyCore = Vector3.zero;
}

[System.Serializable]
public class ItemSource
{
    [SerializeField] public string name = "種別";
    [SerializeField] public Material material = default;
    [SerializeField] public ParticleSystem vfx = default;
    [SerializeField] public AudioSource sfx = default;
    [SerializeField] public float duration = 0.2f;
    [SerializeField] public string description = "説明";
    [SerializeField] public int unitPrice = 0;
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
    [SerializeField] public EnemyType[] types;
}

[System.Serializable]
public class ItemTypesSource
{
    [SerializeField] public ItemType[] randomTypes;
    [SerializeField] public ItemType[] singleTypes;
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
        this.spriteImage = spriteImage;
        this.matImage = matImage;
        this.caption = caption;
        this.ignoreIfRead = ignoreIfRead;
    }

    public MessageData Convert()
        => new MessageData(sentence.Replace("\\n", "\n"), face, fontSize, literalsPerSec, alignment, spriteImage, matImage, caption, ignoreIfRead);
}

[System.Serializable]
public class FloorMessagesSource
{
    [SerializeField] public BoardMessageData[] fixedMessages = default;
    [SerializeField] public BoardMessageData[] randomMessages = default;
}
