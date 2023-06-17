public class GhostMapUtil : EnemyMapUtil
{

    public override bool IsMovable(Pos destPos, IDirection dir = null)
    {
        ITile tile = map.GetTile(destPos);
        return tile.IsViewOpen && !tile.IsCharacterOn;
    }
}