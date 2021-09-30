using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;

public class HidePlateHandler : MonoBehaviour
{
    [SerializeField] private HidePlatePool hidePlatePool;
    [SerializeField] private GameObject plateFrontPrefab = default;
    [SerializeField] private MiniMap miniMap = default;

    protected GameObject plateFront;

    protected LandscapeUpdater landscape = null;
    protected Dictionary<IDirection, PlateUpdater> portrait = new Dictionary<IDirection, PlateUpdater>();
    protected Dictionary<IDirection, Quaternion> rotatePlateFront = new Dictionary<IDirection, Quaternion>();
    protected Dictionary<IDirection, Pos> vecPlateFront = new Dictionary<IDirection, Pos>();

    private PlateUpdater currentUpdater = null;

    private WorldMap map;
    private MapUtil mapUtil;

    protected const int RANGE = 11;
    protected const int WIDTH = 7;
    protected const int HEIGHT = 15;

    private Pos prevPos = new Pos();
    private Pos CurrentPos => map.MapPos(transform.position);

    private Pos currentOffset = new Pos();
    private Quaternion currentRotate = Quaternion.identity;

    void Awake()
    {
        map = GameManager.Instance.worldMap;
        mapUtil = GetComponent<MapUtil>();

        plateFront = Instantiate(plateFrontPrefab, Vector3.zero, Quaternion.identity);

        landscape = new LandscapeUpdater(this, RANGE);
        portrait[Direction.north] = new PortraitNUpdater(this, WIDTH, HEIGHT);
        portrait[Direction.east] = new PortraitEUpdater(this, HEIGHT, WIDTH);
        portrait[Direction.south] = new PortraitSUpdater(this, WIDTH, HEIGHT);
        portrait[Direction.west] = new PortraitWUpdater(this, HEIGHT, WIDTH);

        rotatePlateFront[Direction.north] = Quaternion.identity;
        rotatePlateFront[Direction.east] = Quaternion.Euler(0f, 90f, 0f);
        rotatePlateFront[Direction.south] = Quaternion.identity;
        rotatePlateFront[Direction.west] = Quaternion.Euler(0f, 90f, 0f);

        vecPlateFront[Direction.north] = new Pos(0, -1);
        vecPlateFront[Direction.east] = new Pos(1, 0);
        vecPlateFront[Direction.south] = new Pos(0, 1);
        vecPlateFront[Direction.west] = new Pos(-1, 0);
    }

    private void UpdateRange(Action<Pos, Plate[,]> DrawAction)
    {
        Pos playerPos = CurrentPos;
        Plate[,] plateMap = currentUpdater.GetPlateMap(playerPos);

        // Update MiniMap at changing player view
        miniMap.UpdateMiniMap();

        DrawAction(playerPos, plateMap);
        MoveOuterPlate(playerPos);

        prevPos = playerPos;
    }

    private void MoveOuterPlate(Pos pos)
    {
        // Move with delay
        DOVirtual.DelayedCall(0.1f, () =>
        {
            plateFront.transform.rotation = currentRotate;
            plateFront.transform.position = map.WorldPos(pos + currentOffset);
        })
        .Play();
    }

    public void Draw()
    {
        UpdateRange((playerPos, plateMap) => currentUpdater.DrawRange(playerPos, plateMap));
    }

    public void Redraw()
    {
        UpdateRange((playerPos, plateMap) => currentUpdater.RedrawRange(playerPos, plateMap));
    }

    public void Move()
    {
        UpdateRange((playerPos, plateMap) => currentUpdater.MoveRange(playerPos, plateMap));
    }


    public void Turn()
    {
        currentRotate = rotatePlateFront[mapUtil.dir];

        if (currentUpdater == landscape)
        {
            currentOffset = vecPlateFront[mapUtil.dir] * (RANGE * 3 / 2 + 1);
            MoveOuterPlate(prevPos);
            return;
        }

        currentOffset = vecPlateFront[mapUtil.dir] * (2 * HEIGHT - WIDTH);

        currentUpdater?.ClearRange(CurrentPos);
        currentUpdater = portrait[mapUtil.dir];
        Draw();

        miniMap.Turn(mapUtil.dir);
    }

    public void Init()
    {
        ReformHidePlates(DeviceOrientation.Portrait);
        miniMap.Turn(mapUtil.dir);
    }

    public void ReformHidePlates(DeviceOrientation orientation)
    {
        currentUpdater?.ClearRange(CurrentPos);

        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                currentUpdater = portrait[mapUtil.dir];
                currentRotate = rotatePlateFront[mapUtil.dir];
                currentOffset = vecPlateFront[mapUtil.dir] * (2 * HEIGHT - WIDTH);
                break;

            case DeviceOrientation.LandscapeRight:
                currentUpdater = landscape;
                currentRotate = rotatePlateFront[mapUtil.dir];
                currentOffset = vecPlateFront[mapUtil.dir] * (RANGE * 3 / 2 + 1);
                break;
        }

        Draw();
    }

    protected abstract class PlateUpdater
    {
        protected HidePlateHandler hidePlateHandler;
        protected HidePlatePool hidePlatePool;
        protected HidePlate SpawnPlate(Plate plate, Pos pos, float duration = 0.01f) => hidePlatePool.SpawnPlate(plate, pos, duration);
        protected WorldMap map;
        protected Pos prevPos => hidePlateHandler.prevPos;
        protected HidePlate[,] plateData;

        protected int width;
        protected int height;
        protected Pos playerOffsetPos;

        protected PlateUpdater(HidePlateHandler hidePlateUpdater, int width, int height)
        {
            this.hidePlateHandler = hidePlateUpdater;
            this.hidePlatePool = hidePlateUpdater.hidePlatePool;

            map = hidePlateUpdater.map;

            int maxRange = 2 * Mathf.Max(width, height) - Mathf.Min(width, height) - 1;
            plateData = new HidePlate[map.Width + maxRange, map.Height + maxRange];

            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Light weight simple version of RedrawRange() method
        /// </summary>
        public void DrawRange(Pos playerPos, Plate[,] plateMap)
        {
            Pos startPos = playerPos - playerOffsetPos;

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    int x = playerPos.x + i;
                    int y = playerPos.y + j;
                    plateData[x, y] = SpawnPlate(plateMap[i, j], startPos.Add(i, j));
                }
            }
        }

        public void RedrawRange(Pos playerPos, Plate[,] plateMap, int xShrink = 0, int yShrink = 0)
        {
            Pos startPos = playerPos - playerOffsetPos;

            for (int j = yShrink; j < height - yShrink; j++)
            {
                for (int i = xShrink; i < width - xShrink; i++)
                {
                    int x = playerPos.x + i;
                    int y = playerPos.y + j;

                    if (plateMap[i, j] != (plateData[x, y]?.plate ?? Plate.NONE))
                    {
                        plateData[x, y]?.Remove(0.4f);
                        plateData[x, y] = SpawnPlate(plateMap[i, j], startPos.Add(i, j), 0.1f);
                    }
                }
            }
        }

        public void ClearRange(Pos playerPos)
        {
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    plateData[playerPos.x + i, playerPos.y + j]?.Remove(0.25f);
                }
            }
        }

        private void RedrawXShrink(Pos playerPos, Plate[,] plateMap) => RedrawRange(playerPos, plateMap, 1, 0);
        private void RedrawYShrink(Pos playerPos, Plate[,] plateMap) => RedrawRange(playerPos, plateMap, 0, 1);

        public void MoveRange(Pos playerPos, Plate[,] plateMap)
        {
            Pos moveVec = playerPos - prevPos;

            if (moveVec.y < 0) MoveRangeNorth(playerPos, plateMap);
            if (moveVec.y > 0) MoveRangeSouth(playerPos, plateMap);
            if (moveVec.x < 0) MoveRangeWest(playerPos, plateMap);
            if (moveVec.x > 0) MoveRangeEast(playerPos, plateMap);
        }

        private void MoveRangeNorth(Pos playerPos, Plate[,] plateMap)
        {
            Pos startPos = playerPos - playerOffsetPos;

            for (int i = 0; i < width; i++)
            {
                plateData[playerPos.x + i, playerPos.y + height]?.Remove();
                plateData[playerPos.x + i, playerPos.y] = SpawnPlate(plateMap[i, 0], startPos.AddX(i));
            }
            RedrawYShrink(playerPos, plateMap);
        }
        private void MoveRangeSouth(Pos playerPos, Plate[,] plateMap)
        {
            Pos startPos = playerPos - playerOffsetPos;

            for (int i = 0; i < width; i++)
            {
                plateData[playerPos.x + i, playerPos.y - 1]?.Remove();
                plateData[playerPos.x + i, playerPos.y + height - 1] = SpawnPlate(plateMap[i, height - 1], startPos.Add(i, height - 1));
            }
            RedrawYShrink(playerPos, plateMap);
        }
        private void MoveRangeWest(Pos playerPos, Plate[,] plateMap)
        {
            Pos startPos = playerPos - playerOffsetPos;

            for (int j = 0; j < height; j++)
            {
                plateData[playerPos.x + width, playerPos.y + j]?.Remove();
                plateData[playerPos.x, playerPos.y + j] = SpawnPlate(plateMap[0, j], startPos.AddY(j));
            }
            RedrawXShrink(playerPos, plateMap);
        }
        private void MoveRangeEast(Pos playerPos, Plate[,] plateMap)
        {
            Pos startPos = playerPos - playerOffsetPos;

            for (int j = 0; j < height; j++)
            {
                plateData[playerPos.x - 1, playerPos.y + j]?.Remove();
                plateData[playerPos.x + width - 1, playerPos.y + j] = SpawnPlate(plateMap[width - 1, j], startPos.Add(width - 1, j));
            }
            RedrawXShrink(playerPos, plateMap);
        }

        /// <summary>
        /// Get Tiles view open inside viewing range for player's see through info
        /// </summary>
        private bool[,] GetTileViewOpen(Pos playerPos)
        {
            int tileWidth = width + 2;
            int tileHeight = height + 2;

            var region = new bool[tileWidth, tileHeight];

            for (int i = 0; i < tileWidth; i++)
            {
                region[i, 0] = region[i, tileHeight - 1] = false;
            }

            for (int j = 1; j < tileHeight - 1; j++)
            {
                region[0, j] = region[tileWidth - 1, j] = false;
            }

            Pos startPos = playerPos - playerOffsetPos.Add(1, 1);

            for (int j = 1; j < tileHeight - 1; j++)
            {
                for (int i = 1; i < tileWidth - 1; i++)
                {
                    region[i, j] = map.IsTileViewOpen(startPos.x + i, startPos.y + j);
                }
            }

            return region;
        }

        /// <summary>
        /// Hide plate placing data
        /// </summary>
        /// <param name="playerPos"></param>
        /// <returns></returns>
        public Plate[,] GetPlateMap(Pos playerPos)
        {
            bool[,] viewOpen = GetTileViewOpen(playerPos);

            var plateMap = new Plate[width, height];

            // Fill tiles inside player view range with HidePlates
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    plateMap[i, j] = Plate.ABCD;
                }
            }

            map.currentViewOpen.Clear();

            var edgePos = playerPos - playerOffsetPos;
            var openStack = new Stack<Pos>();

            // Start from player tile
            for (openStack.Push(playerOffsetPos); openStack.Count > 0;)
            {
                Pos pos = openStack.Pop();

                // Set open plate as discovered area
                map.SetDiscovered(edgePos + pos);

                // Delete focused hide plate
                plateMap[pos.x, pos.y] = Plate.NONE;

                // plateMap index[x, y] = region index[x+1, y+1]
                int ptX = pos.x + 1;
                int ptY = pos.y + 1;

                viewOpen[ptX, ptY] = false;

                // Delete corner of hide plates nearby focusing tile
                if (pos.x - 1 >= 0 && pos.y - 1 >= 0)
                {
                    plateMap[pos.x - 1, pos.y - 1] &= ~Plate.D;
                }

                if (pos.x - 1 >= 0 && pos.y + 1 < height)
                {
                    plateMap[pos.x - 1, pos.y + 1] &= ~Plate.B;
                }

                if (pos.x + 1 < width && pos.y - 1 >= 0)
                {
                    plateMap[pos.x + 1, pos.y - 1] &= ~Plate.C;
                }

                if (pos.x + 1 < width && pos.y + 1 < height)
                {
                    plateMap[pos.x + 1, pos.y + 1] &= ~Plate.A;
                }

                // Push neighbor tile as hide plate deleting candidate
                // If not a delete candidate, delete only a half of the plate
                if (viewOpen[ptX - 1, ptY])
                {
                    openStack.Push(pos.DecX());
                }
                else if (pos.x - 1 >= 0)
                {
                    plateMap[pos.x - 1, pos.y] &= ~Plate.BD;
                }

                if (viewOpen[ptX, ptY - 1])
                {
                    openStack.Push(pos.DecY());
                }
                else if (pos.y - 1 >= 0)
                {
                    plateMap[pos.x, pos.y - 1] &= ~Plate.CD;
                }

                if (viewOpen[ptX + 1, ptY])
                {
                    openStack.Push(pos.IncX());
                }
                else if (pos.x + 1 < width)
                {
                    plateMap[pos.x + 1, pos.y] &= ~Plate.AC;
                }

                if (viewOpen[ptX, ptY + 1])
                {
                    openStack.Push(pos.IncY());
                }
                else if (pos.y + 1 < height)
                {
                    plateMap[pos.x, pos.y + 1] &= ~Plate.AB;
                }
            }

            return plateMap;
        }
    }

    protected class LandscapeUpdater : PlateUpdater
    {
        public LandscapeUpdater(HidePlateHandler hidePool, int range) : base(hidePool, range, range)
        {
            playerOffsetPos = new Pos(range / 2, range / 2);
        }

    }
    protected class PortraitNUpdater : PlateUpdater
    {
        public PortraitNUpdater(HidePlateHandler hidePool, int width, int height) : base(hidePool, width, height)
        {
            playerOffsetPos = new Pos(width / 2, height - width / 2 - 1);
        }
    }
    protected class PortraitSUpdater : PlateUpdater
    {
        public PortraitSUpdater(HidePlateHandler hidePool, int width, int height) : base(hidePool, width, height)
        {
            playerOffsetPos = new Pos(width / 2, width / 2);
        }
    }
    protected class PortraitEUpdater : PlateUpdater
    {
        public PortraitEUpdater(HidePlateHandler hidePool, int width, int height) : base(hidePool, width, height)
        {
            playerOffsetPos = new Pos(height / 2, height / 2);
        }
    }

    protected class PortraitWUpdater : PlateUpdater
    {
        public PortraitWUpdater(HidePlateHandler hidePool, int width, int height) : base(hidePool, width, height)
        {
            playerOffsetPos = new Pos(width - height / 2 - 1, height / 2);
        }
    }
}
