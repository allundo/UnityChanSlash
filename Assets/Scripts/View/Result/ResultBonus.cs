using UnityEngine;

public struct ResultBonus
{
    private GameInfo gameInfo;

    public ulong itemPrice { get; private set; }

    public ulong mapComp => (ulong)(gameInfo.mapComp * 1000f);
    public int mapCompBonus { get; private set; }

    public ulong clearTimeSec => (ulong)gameInfo.endTimeSec;
    public int clearTimeBonus { get; private set; }

    public ulong defeatCount => (ulong)gameInfo.defeatCount;
    public int defeatBonus { get; private set; }

    public ulong level => (ulong)gameInfo.level;
    public int levelBonus { get; private set; }

    public ulong strength => (ulong)gameInfo.strength;
    public int strengthBonus { get; private set; }

    public ulong magic => (ulong)gameInfo.magic;
    public int magicBonus { get; private set; }

    public string title { get; private set; }
    public int bonusPay { get; private set; }

    public ulong wagesAmount { get; private set; }


    public ResultBonus(GameInfo gameInfo)
    {
        this.gameInfo = gameInfo;

        itemPrice = gameInfo.moneyAmount;
        mapCompBonus = (int)(100000f * gameInfo.mapComp * gameInfo.mapComp);
        int endTimeSec = gameInfo.endTimeSec;
        clearTimeBonus = endTimeSec > 7200 ? 0 : 5184000 - (endTimeSec * endTimeSec / 10);
        defeatBonus = gameInfo.defeatCount * 100;
        levelBonus = gameInfo.level * 1000;
        strengthBonus = gameInfo.strength * 100;
        magicBonus = gameInfo.magic * 100;

        title = gameInfo.title;

        var tempAmount = itemPrice + (ulong)(mapCompBonus + clearTimeBonus + defeatBonus + levelBonus + strengthBonus + magicBonus);
        bonusPay = (int)(tempAmount * gameInfo.titleBonusRatio);
        wagesAmount = tempAmount + (ulong)bonusPay;
    }
}
