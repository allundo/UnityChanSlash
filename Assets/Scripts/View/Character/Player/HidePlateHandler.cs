using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Hides invisible area for player character using HidePlate(hides 1 Tile)s. <br />
/// </summary>
public class HidePlateHandler : MonoBehaviour
{
    [SerializeField] private HidePlatePool hidePlatePool = default;

    /// <summary>
    /// Large hiding plate that hides far front area.
    /// </summary>
    [SerializeField] private GameObject plateFrontPrefab = default;

    /// <summary>
    /// Turn PlayerSymbol direction with player's turn and update discovered area when calcurating visible area.
    /// </summary>
    [SerializeField] private MiniMapHandler miniMap = default;

    protected GameObject plateFront;

    // Sub classes having methods for updating HidePlates position.
    protected LandscapeUpdater landscape = null;
    protected Dictionary<IDirection, PlateUpdater> portrait = new Dictionary<IDirection, PlateUpdater>();


    /// <summary>
    /// Quaternion rotations applies to PlateFront for each direction.
    /// </summary>
    protected Dictionary<IDirection, Quaternion> rotatePlateFront = new Dictionary<IDirection, Quaternion>();

    /// <summary>
    /// Normalized tile vectors expresses direction.
    /// </summary>
    protected Dictionary<IDirection, Pos> vecPlateFront = new Dictionary<IDirection, Pos>();

    /// <summary>
    /// Set proper updater from variants.
    /// </summary>
    private PlateUpdater currentUpdater = null;

    /// <summary>
    /// FIXME: Directly handling WorldMap MODEL data by this VIEW class for now. <br />
    /// (Could be regarded as PRESENTER class?)
    /// </summary>
    private WorldMap map;

    /// <summary>
    /// Player's direction related data for Turn.
    /// </summary>
    private MapUtil mapUtil;

    protected const int RANGE = 11;
    protected const int WIDTH = 7;
    protected const int HEIGHT = 15;

    /// <summary>
    /// Previous player position for calcurate moved direction and distance.
    /// </summary>
    private Pos prevPos = new Pos();

    /// <summary>
    /// Calcurate current player's map tile position
    /// </summary>
    private Pos CurrentPos => map.MapPos(transform.position);

    /// <summary>
    /// Current tile map offset position of PlateFront from player position.
    /// </summary>
    private Pos currentOffset = new Pos();
    /// <summary>
    /// Current rotation of PlateFront decided by player's direction.
    /// </summary>
    private Quaternion currentRotate = Quaternion.identity;

    void Awake()
    {
        map = GameManager.Instance.worldMap;
        mapUtil = GetComponent<PlayerMapUtil>();

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
        MovePlateFront(playerPos);

        prevPos = playerPos;
    }

    private void MovePlateFront(Pos pos)
    {
        // Move with delay
        DOVirtual.DelayedCall(0.1f, () =>
        {
            plateFront.transform.rotation = currentRotate;
            plateFront.transform.position = map.WorldPos(pos + currentOffset);
        })
        .Play();
    }

    /// <summary>
    /// Simple whole HidePlates drawing inside player viewing range. <br />
    /// Can be used only when initial state or all HidePlates are cleared.
    /// </summary>
    private void Draw()
    {
        UpdateRange((playerPos, plateMap) => currentUpdater.DrawRange(playerPos, plateMap));
    }

    /// <summary>
    /// Draws or updates whole HidePlates inside player viewing range. <br />
    /// Scan all plates and switch the shape of HidePlates if needed.
    /// </summary>
    public void Redraw()
    {
        UpdateRange((playerPos, plateMap) => currentUpdater.RedrawRange(playerPos, plateMap));
    }

    /// <summary>
    /// Updates HidePlates which is the edges of viewing range. <br />
    /// Remove the backward edge and newly draw the forward edge according to moving direction.
    /// </summary>
    public void Move()
    {
        UpdateRange((playerPos, plateMap) => currentUpdater.MoveRange(playerPos, plateMap));
    }


    /// <summary>
    /// Turn the HidePlates with player's turning. <br />
    /// </summary>
    public void Turn()
    {
        currentRotate = rotatePlateFront[mapUtil.dir];
        miniMap.Turn(mapUtil.dir);

        if (currentUpdater == landscape)
        {
            currentOffset = vecPlateFront[mapUtil.dir] * (RANGE * 3 / 2 + 1);
            MovePlateFront(prevPos);
            return;
        }

        currentOffset = vecPlateFront[mapUtil.dir] * (2 * HEIGHT - WIDTH);

        currentUpdater?.ClearRange(CurrentPos);
        currentUpdater = portrait[mapUtil.dir];
        Draw();
    }

    /// <summary>
    /// Initial drawing of HidePlates
    /// </summary>
    public void Init()
    {
        ReformHidePlates(DeviceOrientation.Portrait);
        miniMap.Turn(mapUtil.dir);
    }

    /// <summary>
    /// Clear and redraw whole HidePlates inside new player view range with newly detected orientation.
    /// </summary>
    /// <param name="orientation"></param>
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

    public void SwitchWorldMap(WorldMap map)
    {
        this.map = map;

        miniMap.SwitchWorldMap(map);

        currentUpdater?.ClearRange(prevPos);
        currentUpdater?.DispRange(prevPos);

        hidePlatePool.SwitchWorldMap(map);
        landscape.ResetWorldMapRange();
        portrait.ForEach(updater => updater.Value.ResetWorldMapRange());
    }

    public void OnStartFloor()
    {
        miniMap.OnStartFloor();
        Turn();
    }

    protected abstract class PlateUpdater
    {
        /// <summary>
        /// Outer class reference
        /// </summary>
        protected HidePlateHandler hidePlateHandler;
        protected Pos prevPos => hidePlateHandler.prevPos;

        /// <summary>
        /// HidePlate spawner
        /// </summary>
        protected HidePlatePool hidePlatePool;
        protected HidePlate SpawnPlate(Plate plate, Pos pos, float duration = 0.01f) => hidePlatePool.SpawnPlate(plate, pos, duration);

        /// <summary>
        /// To get tile map info and to set player visible range info
        /// </summary>
        protected WorldMap map;

        /// <summary>
        /// HidePlates data covers drawing range. <br />
        /// Tile map width x height + player view distance will be reserved. <br />
        /// </summary>
        protected HidePlate[,] plateData;

        /// <summary>
        /// Tile map player view range width
        /// </summary>
        protected int width;
        /// <summary>
        /// Tile map player view range height
        /// </summary>
        protected int height;

        /// <summary>
        /// Tile map offset position of player from HidePlate drawing start position(left,top origin)
        /// </summary>
        protected Pos playerOffsetPos;

        /// <summary>
        ///
        /// </summary>
        /// <param name="hidePlateHandler">Outer class reference</param>
        /// <param name="width">Player view range width</param>
        /// <param name="height">Player view range height</param>
        protected PlateUpdater(HidePlateHandler hidePlateHandler, int width, int height)
        {
            this.hidePlateHandler = hidePlateHandler;
            this.hidePlatePool = hidePlateHandler.hidePlatePool;

            this.width = width;
            this.height = height;

            ResetWorldMapRange();
        }

        public void ResetWorldMapRange()
        {
            map = hidePlateHandler.map;
            int maxRange = 2 * Mathf.Max(width, height) - Mathf.Min(width, height) - 1;
            plateData = new HidePlate[map.Width + maxRange, map.Height + maxRange];
        }

        /// <summary>
        /// Simple plates drawing inside the range.<br />
        /// Caution: draws plate regardless of existing plates.
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

        /// <summary>
        /// Draw or update plates drawing inside the range.<br />
        /// </summary>
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

        /// <summary>
        /// Clear all plates drawing inside the range.<br />
        /// </summary>
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

        public void DispRange(Pos playerPos)
        {
            int maxRange = 2 * Mathf.Max(width, height) - Mathf.Min(width, height) - 1;
            Debug.Log("PlateRange: " + (maxRange + map.Width) + " | width = " + width + ", height = " + height + " | PlayerPos: x = " + playerPos.x + ", y = " + playerPos.y);
        }

        private void RedrawXShrink(Pos playerPos, Plate[,] plateMap) => RedrawRange(playerPos, plateMap, 1, 0);
        private void RedrawYShrink(Pos playerPos, Plate[,] plateMap) => RedrawRange(playerPos, plateMap, 0, 1);

        /// <summary>
        /// Update the both edge of HidePlates inside player view range.
        /// </summary>
        public void MoveRange(Pos playerPos, Plate[,] plateMap)
        {
            Pos moveVec = playerPos - prevPos;

            if (moveVec.y < 0) MoveRangeNorth(playerPos, plateMap);
            if (moveVec.y > 0) MoveRangeSouth(playerPos, plateMap);
            if (moveVec.x < 0) MoveRangeWest(playerPos, plateMap);
            if (moveVec.x > 0) MoveRangeEast(playerPos, plateMap);
        }

        /// <summary>
        /// Delete south edge and newly draw north edge
        /// </summary>
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
        /// <summary>
        /// Delete north edge and newly draw south edge
        /// </summary>
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
        /// <summary>
        /// Delete east edge and newly draw west edge
        /// </summary>
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
        /// <summary>
        /// Delete west edge and newly draw east edge
        /// </summary>
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
        /// HidePlates shape data for drawing range
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
