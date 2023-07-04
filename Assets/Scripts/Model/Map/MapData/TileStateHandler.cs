using System.Linq;
using System.Collections.Generic;

public interface ITileStateData
{
    List<Pos> open { get; }
    List<Pos> broken { get; }
    List<Pos> read { get; }
    static bool isExitDoorLocked = true;
}

public class TileStateData : ITileStateData
{
    public List<Pos> open { get; protected set; }
    public List<Pos> broken { get; protected set; }
    public List<Pos> read { get; protected set; }

    public TileStateData(List<Pos> tileOpenPos, List<Pos> tileBrokenPos, List<Pos> messageReadPos)
    {
        open = tileOpenPos;
        broken = tileBrokenPos;
        read = messageReadPos;
    }
}

public class TileStateHandler : TileMapHandler
{
    private TileStateData data = null;

    protected Pos exitDoor = new Pos();

    public TileStateHandler(ITile[,] matrix, int floor, int width, int height) : base(matrix, floor, width, height)
    { }

    public void SetExitDoor(Pos exitDoor)
    {
        this.exitDoor = exitDoor;
    }

    public void ApplyTileState()
    {
        if (data != null)
        {
            data.open.ForEach(pos => (matrix[pos.x, pos.y] as IOpenable).Open());
            data.broken.ForEach(pos => (matrix[pos.x, pos.y] as Door).Break());
            data.read.ForEach(pos => (matrix[pos.x, pos.y] as IReadable).Read());
        }

        if (floor == 1 && !ITileStateData.isExitDoorLocked)
        {
            var door = (matrix[exitDoor.x, exitDoor.y] as ExitDoor);
            if (!door.IsOpen && !door.IsBroken) door.Unlock();
        }
    }

    public void Import(Pos[] open, Pos[] broken, Pos[] read)
    {
        data = new TileStateData(open.ToList(), broken.ToList(), read.ToList());
    }

    public TileStateData ExportTileStateData()
    {
        return data ?? RetrieveTileStateData();
    }

    public void StoreTileStateData()
    {
        data = RetrieveTileStateData();
    }

    public TileStateData RetrieveTileStateData()
    {
        var open = new List<Pos>();
        var broken = new List<Pos>();
        var read = new List<Pos>();

        ForEachTiles((tile, pos) =>
        {
            if (tile is IOpenable)
            {
                if (tile is Door && (tile as Door).IsBroken)
                {
                    broken.Add(pos);
                }
                else if ((tile as IOpenable).IsOpen)
                {
                    open.Add(pos);
                }
            }

            if (tile is IReadable && (tile as IReadable).IsRead) read.Add(pos);
        });

        if (floor == 1)
        {
            ITileStateData.isExitDoorLocked = (matrix[exitDoor.x, exitDoor.y] as ExitDoor).IsLocked;
        }

        return new TileStateData(open, broken, read);
    }
    public void ClearCharacterOnTileInfo()
    {
        ForEachTiles(tile => tile.OnCharacterDest = tile.OnEnemy = tile.AboveEnemy = null);
    }
}
