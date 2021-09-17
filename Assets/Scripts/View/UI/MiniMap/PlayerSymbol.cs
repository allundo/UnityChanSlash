using System.Collections.Generic;
using UnityEngine;
public class PlayerSymbol : UISymbol
{
    private Dictionary<IDirection, Quaternion> angles = new Dictionary<IDirection, Quaternion>();

    void Start()
    {
        angles[Direction.north] = Quaternion.identity;
        angles[Direction.east] = Quaternion.Euler(0, 0, -90);
        angles[Direction.south] = Quaternion.Euler(0, 0, 180);
        angles[Direction.west] = Quaternion.Euler(0, 0, 90);
    }

    public PlayerSymbol SetDir(IDirection dir)
    {
        rectTransform.rotation = angles[dir];
        return this;
    }
}