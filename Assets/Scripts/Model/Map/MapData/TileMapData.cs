using UnityEngine;
using System;

public class TileMapData : BaseMapData<ITile>
{
    public static readonly float TILE_UNIT = Constants.TILE_UNIT;

    public int floor { get; protected set; } = 0;

    protected TileMapData(ITile[,] matrix, int floor, int width, int height) : base(matrix, width, height)
    {
        this.floor = floor;
    }

    public Pos MapPos(Vector3 pos) =>
        new Pos(
            (int)Math.Round((width - 1) * 0.5f + pos.x / TILE_UNIT, MidpointRounding.AwayFromZero),
            (int)Math.Round((height - 1) * 0.5f - pos.z / TILE_UNIT, MidpointRounding.AwayFromZero)
        );

    public Vector3 WorldPos(Pos pos) => WorldPos(pos.x, pos.y);
    public Vector3 WorldPos(int x, int y) => new Vector3((0.5f + x - width * 0.5f) * TILE_UNIT, 0.0f, (-0.5f - y + height * 0.5f) * TILE_UNIT);

    public bool IsOutOfRange(int x, int y) => x < 0 || y < 0 || x >= width || y >= height;

    public static ITile ConvertToTile(Terrain terrain, int floor)
    {
        switch (terrain)
        {
            case Terrain.Wall:
            case Terrain.Pillar:
                return new Wall();

            case Terrain.Table:
                return new Furniture(new ActiveMessageData("これはダイニングテーブル・・・？", SDFaceID.DEFAULT, SDEmotionID.QUESTION));

            case Terrain.Chair:
                return new Furniture(new ActiveMessageData("テーブル越しに椅子が向かい合って置かれている。", SDFaceID.DEFAULT, SDEmotionID.QUESTION));

            case Terrain.Cabinet:
                return new Furniture(new ActiveMessageData("どうやら食器棚のようだ。", SDFaceID.SURPRISE, SDEmotionID.QUESTION));

            case Terrain.Bed:
                return new Furniture(new ActiveMessageData("ベッドが使用されている様子はない。", SDFaceID.DEFAULT, SDEmotionID.QUESTION));

            case Terrain.Fountain:
                return new Fountain(
                    new ActiveMessageData("魔力を帯びた水が流れ出ている。", SDFaceID.SURPRISE, SDEmotionID.SURPRISE),
                    new ActiveMessageData("禍々しい液体が流れ出ている。", SDFaceID.SURPRISE, SDEmotionID.EXQUESTION)
                );

            case Terrain.MessageWall:
            case Terrain.MessagePillar:
            case Terrain.BloodMessageWall:
            case Terrain.BloodMessagePillar:
                return new MessageWall();

            case Terrain.Door:
                return new Door();
            case Terrain.LockedDoor:
                return new Door(ItemType.TreasureKey);

            case Terrain.ExitDoor:
                return new ExitDoor();

            case Terrain.DownStairs:
            case Terrain.UpStairs:
                return new Stairs();

            case Terrain.Box:
                return new Box();

            case Terrain.Pit:
                return new Pit(floor);

            default:
                return new Ground();
        }
    }
}
