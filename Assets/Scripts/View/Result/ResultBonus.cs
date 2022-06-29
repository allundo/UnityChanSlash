using UnityEngine;

public struct ResultBonus
{
    private GameInfo gameInfo;

    public ulong itemPrice { get; private set; }

    public ulong mapComp => (ulong)(gameInfo.mapComp * 1000f);
    public int mapCompBonus { get; private set; }

    public ulong clearTimeSec => (ulong)gameInfo.clearTimeSec;
    public int clearTimeBonus { get; private set; }

    public ulong defeatCount => (ulong)gameInfo.defeatCount;
    public int defeatBonus { get; private set; }

    public ulong level => (ulong)gameInfo.level;
    public int levelBonus { get; private set; }

    public ulong strength => (ulong)gameInfo.strength;
    public int strengthBonus { get; private set; }

    public ulong magic => (ulong)gameInfo.magic;
    public int magicBonus { get; private set; }

    public ulong wagesAmount { get; private set; }

    public ResultBonus(GameInfo gameInfo)
    {
        this.gameInfo = gameInfo;

        itemPrice = gameInfo.moneyAmount;
        mapCompBonus = (int)(1000000f * gameInfo.mapComp);
        clearTimeBonus = (int)(10000f * (Mathf.Max(0, 3600 - gameInfo.clearTimeSec)));
        defeatBonus = gameInfo.defeatCount * 1000;
        levelBonus = gameInfo.level * 1000;
        strengthBonus = gameInfo.strength * 1000;
        magicBonus = gameInfo.magic * 1000;

        wagesAmount = itemPrice + (ulong)(mapCompBonus + clearTimeBonus + defeatBonus + levelBonus + strengthBonus + magicBonus);
    }
}