using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

/// <summary>
/// Hides invisible area for player character using HidePlate(hides 1 Tile)s. <br />
/// </summary>
public class HidePlateHandler : MonoBehaviour
{
    [SerializeField] private HidePlatePool hidePlatePool = default;

    /// <summary>
    /// Turn PlayerSymbol direction with player's turn and update discovered area when calculating visible area.
    /// </summary>
    [SerializeField] private MiniMapHandler miniMap = default;

    /// <summary>
    /// Large hiding plate that hides far front area.
    /// </summary>
    protected HidePlateFront plateFront => hidePlatePool.plateFront;

    // Sub classes having methods for updating HidePlates position.
    protected LandscapeUpdater landscape = null;
    protected Dictionary<IDirection, PlateUpdater> portrait = new Dictionary<IDirection, PlateUpdater>();

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
    private IMapUtil mapUtil;

    protected const int RANGE = 11;
    protected const int WIDTH = 9;
    protected const int HEIGHT = 15;
    protected const int REAR = 3;

    /// <summary>
    /// Previous player position for calculate moved direction and distance.
    /// </summary>
    private Pos prevPos = new Pos();

    /// <summary>
    /// Calculate current player's map tile position
    /// </summary>
    private Pos CurrentPos => map.MapPos(transform.position);

    void Awake()
    {
        map = GameManager.Instance.worldMap;
        mapUtil = GetComponent<PlayerMapUtil>();

        landscape = new LandscapeUpdater(this, RANGE);
        portrait[Direction.north] = new PortraitNUpdater(this, WIDTH, HEIGHT, REAR);
        portrait[Direction.east] = new PortraitEUpdater(this, HEIGHT, WIDTH, REAR);
        portrait[Direction.south] = new PortraitSUpdater(this, WIDTH, HEIGHT, REAR);
        portrait[Direction.west] = new PortraitWUpdater(this, HEIGHT, WIDTH, REAR);
    }

    private void UpdateRange(Action<Pos, Plate[,]> DrawAction)
    {
        Pos playerPos = CurrentPos;
        Plate[,] plateMap = currentUpdater.GetPlateMap(playerPos);

        // Update MiniMap at changing player view
        miniMap.UpdateMiniMap();

        DrawAction(playerPos, plateMap);
        plateFront.Move(playerPos);

        prevPos = playerPos;
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
    public void Turn(bool redrawOnLandscape = false)
    {
        plateFront.SetRotation(mapUtil.dir);
        miniMap.Turn(mapUtil.dir);

        if (currentUpdater == landscape)
        {
            plateFront.SetLandscapeOffset(mapUtil.dir);
            plateFront.Move(prevPos);
            if (redrawOnLandscape) Draw();
            return;
        }

        plateFront.SetPortraitOffset(mapUtil.dir);

        currentUpdater?.ClearRange(CurrentPos);
        currentUpdater = portrait[mapUtil.dir];
        Draw();
    }

    /// <summary>
    /// Initial drawing of HidePlates
    /// </summary>
    public void Init()
    {
        plateFront.InitPlateSize(RANGE, HEIGHT, REAR);
        ReformHidePlates(DeviceOrientation.Portrait);
        miniMap.Turn(mapUtil.dir);
    }

    /// <summary>
    /// Clear and redraw whole HidePlates inside new player view range with newly detected orientation.
    /// </summary>
    /// <param name="orientation"></param>
    public void ReformHidePlates(DeviceOrientation orientation)
    {
        currentUpdater?.ClearRangeImmediately(CurrentPos);
        plateFront.SetRotation(mapUtil.dir);

        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                currentUpdater = portrait[mapUtil.dir];
                plateFront.SetPortraitOffset(mapUtil.dir);
                break;

            case DeviceOrientation.LandscapeRight:
                currentUpdater = landscape;
                plateFront.SetLandscapeOffset(mapUtil.dir);
                break;
        }

        Draw();
    }

#if UNITY_EDITOR
    public void Teleport()
    {
        currentUpdater?.ClearRangeImmediately(prevPos);
        Draw();
    }
#endif

    public void SwitchWorldMap(WorldMap map)
    {
        this.map = map;

        miniMap.SwitchWorldMap(map);

        currentUpdater?.ClearRangeImmediately(prevPos);
        hidePlatePool.SwitchWorldMap(map);

        landscape.ResetWorldMapRange();
        portrait.ForEach(updater => updater.Value.ResetWorldMapRange());
    }

    public void OnMoveFloor() => miniMap.OnMoveFloor();
    public void OnStartFloor()
    {
        miniMap.OnStartFloor();
        Turn(true);
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

                    Plate src = plateMap[i, j];
                    Plate dst = plateData[x, y]?.plate ?? Plate.NONE;

                    if (src != dst)
                    {
                        // Remove the old plate simply if the new plate is NONE
                        if (src == Plate.NONE)
                        {
                            plateData[x, y]?.Remove(0.4f);
                            plateData[x, y] = null;
                            continue;
                        }

                        Pos pos = startPos.Add(i, j);
                        HidePlate plate = SpawnPlate(src, pos, 0f);

                        // If the new plate expands the old plate,
                        // spawn a dummy plate of expansion part and
                        // replace them with the new plate later.
                        Plate expand = src & ~dst;
                        if (expand != Plate.NONE)
                        {
                            plate.SetAlpha(0f);
                            SpawnPlate(expand, pos, 0.2f).RemoveTimer(0.2f).OnComplete(() => plate.SetAlpha(1f)).Play();

                            plateData[x, y]?.RemoveTimer(0.2f)?.Play();
                            plateData[x, y] = plate;
                            continue;
                        }

                        // If the new plate shrinks the old plate,
                        // replace the old plate with the new plate immediately and
                        // spawn and remove a dummy plate of shrink part.
                        Plate remove = dst & ~src;
                        if (remove != Plate.NONE)
                        {
                            SpawnPlate(remove, pos, 0f).Remove(0.4f);
                            plateData[x, y]?.Inactivate();
                            plateData[x, y] = plate;
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clear all plates drawing inside the range.<br />
        /// </summary>
        public void ClearRange(Pos playerPos, Action<HidePlate> deleteAction = null)
        {
            deleteAction = deleteAction ?? (plate => plate?.Remove(0.25f));

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    deleteAction(plateData[playerPos.x + i, playerPos.y + j]);
                }
            }
        }

        public void ClearRangeImmediately(Pos playerPos)
            => ClearRange(playerPos, plate => plate?.RemoveImmediately());

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

            map.ClearCurrentViewOpen();

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
        public PortraitNUpdater(HidePlateHandler hidePool, int width, int height, int rear) : base(hidePool, width, height)
        {
            playerOffsetPos = new Pos(width / 2, height - rear - 1);
        }
    }
    protected class PortraitSUpdater : PlateUpdater
    {
        public PortraitSUpdater(HidePlateHandler hidePool, int width, int height, int rear) : base(hidePool, width, height)
        {
            playerOffsetPos = new Pos(width / 2, rear);
        }
    }
    protected class PortraitEUpdater : PlateUpdater
    {
        public PortraitEUpdater(HidePlateHandler hidePool, int width, int height, int rear) : base(hidePool, width, height)
        {
            playerOffsetPos = new Pos(rear, height / 2);
        }
    }

    protected class PortraitWUpdater : PlateUpdater
    {
        public PortraitWUpdater(HidePlateHandler hidePool, int width, int height, int rear) : base(hidePool, width, height)
        {
            playerOffsetPos = new Pos(width - rear - 1, height / 2);
        }
    }
}
