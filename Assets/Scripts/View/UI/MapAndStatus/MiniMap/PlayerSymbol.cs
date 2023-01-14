using System.Collections.Generic;
using UnityEngine;
public class PlayerSymbol : UISymbol
{
    private static readonly Dictionary<IDirection, Quaternion> angles =
        new Dictionary<IDirection, Quaternion>()
        {
            {Direction.north, Quaternion.identity},
            {Direction.east, Quaternion.Euler(0, 0, -90)},
            {Direction.south, Quaternion.Euler(0, 0, 180)},
            {Direction.west, Quaternion.Euler(0, 0, 90)},
        };

    public PlayerSymbol SetDir(IDirection dir)
    {
        rectTransform.rotation = angles[dir];
        return this;
    }
}