using UnityEngine;
using System;
using System.Collections.Generic;

public class HidePool : MonoBehaviour
{
    [SerializeField] private GameObject plate5 = default;
    [SerializeField] private GameObject plate4 = default;
    [SerializeField] private GameObject plate3 = default;
    [SerializeField] private GameObject plate2 = default;
    [SerializeField] private GameObject plate1 = default;

    protected Transform[] pool = new Transform[0b10000];
    protected GameObject[] prefab = new GameObject[0b10000];
    protected Quaternion[] rotate = new Quaternion[0b10000];

    protected LandscapeUpdater landscape = null;
    protected HidePlateUpdater portrateN = null;
    protected HidePlateUpdater portrateE = null;
    protected HidePlateUpdater portrateS = null;
    protected HidePlateUpdater portrateW = null;

    private HidePlateUpdater currentUpdater = null;

    private WorldMap map;
    private MapUtil mapUtil;

    protected const int RANGE = 11;
    protected HidePlate[,] plateData;

    private Pos prevPos = new Pos();
    private Pos CurrentPos => map.MapPos(transform.position);

    void Awake()
    {
        map = GameManager.Instance.worldMap;
        mapUtil = GetComponent<MapUtil>();
        InitHidePlates();

        landscape = new LandscapeUpdater(this, RANGE);
        currentUpdater = landscape;
    }

    private void InitHidePlates()
    {
        pool[(int)Plate.A] = pool[(int)Plate.B] = pool[(int)Plate.D] = pool[(int)Plate.C] = new GameObject("Plate1").transform;
        pool[(int)Plate.AB] = pool[(int)Plate.BD] = pool[(int)Plate.CD] = pool[(int)Plate.AC] = new GameObject("Plate2").transform;
        pool[(int)Plate.ABD] = pool[(int)Plate.BCD] = pool[(int)Plate.ACD] = pool[(int)Plate.ABC] = new GameObject("Plate3").transform;
        pool[(int)Plate.ABCD] = new GameObject("PlateFull").transform;
        pool[(int)Plate.AD] = pool[(int)Plate.BC] = new GameObject("PlateCross").transform;

        prefab[(int)Plate.A] = prefab[(int)Plate.B] = prefab[(int)Plate.D] = prefab[(int)Plate.C] = plate1;
        prefab[(int)Plate.AB] = prefab[(int)Plate.BD] = prefab[(int)Plate.CD] = prefab[(int)Plate.AC] = plate2;
        prefab[(int)Plate.ABD] = prefab[(int)Plate.BCD] = prefab[(int)Plate.ACD] = prefab[(int)Plate.ABC] = plate3;
        prefab[(int)Plate.ABCD] = plate4;
        prefab[(int)Plate.AD] = prefab[(int)Plate.BC] = plate5;

        rotate[(int)Plate.A] = rotate[(int)Plate.AB] = rotate[(int)Plate.ABC] = rotate[(int)Plate.ABCD] = rotate[(int)Plate.AD] = Quaternion.identity;
        rotate[(int)Plate.B] = rotate[(int)Plate.BD] = rotate[(int)Plate.ABD] = rotate[(int)Plate.BC] = Quaternion.Euler(0, 90, 0);
        rotate[(int)Plate.D] = rotate[(int)Plate.CD] = rotate[(int)Plate.BCD] = Quaternion.Euler(0, 180, 0);
        rotate[(int)Plate.C] = rotate[(int)Plate.AC] = rotate[(int)Plate.ACD] = Quaternion.Euler(0, 270, 0);
    }

    private HidePlate GetInstance(Plate plate, Pos pos, float duration = 0.01f)
    {
        if (plate == Plate.NONE) return null;

        int id = (int)plate;

        Vector3 worldPos = map.WorldPos(pos);

        foreach (Transform t in pool[id])
        {
            HidePlate hp = t.GetComponent<HidePlate>();

            if (!hp.IsActive)
            {
                t.SetPositionAndRotation(worldPos, rotate[id]);
                hp.plate = plate;
                return hp.FadeIn(duration);
            }
        }

        return HidePlate.GetInstance(prefab[id], worldPos, rotate[id], pool[id], plate);
    }

    private void UpdateRange(Action<Pos, Plate[,]> DrawAction)
    {
        Pos playerPos = CurrentPos;
        Plate[,] plateMap = currentUpdater.GetPlateMap(playerPos);

        DrawAction(playerPos, plateMap);

        prevPos = playerPos;
    }

    public void Init()
    {
        UpdateRange((playerPos, plateMap) => currentUpdater.InitRange(playerPos, plateMap));
    }

    public void Redraw()
    {
        UpdateRange((playerPos, plateMap) => currentUpdater.RedrawRange(playerPos, plateMap));
    }

    public void Move()
    {
        UpdateRange((playerPos, plateMap) => currentUpdater.MoveRange(playerPos, plateMap));
    }

    protected abstract class HidePlateUpdater
    {
        protected HidePool hidePool;
        protected HidePlate GetInstance(Plate plate, Pos pos, float duration = 0.01f) => hidePool.GetInstance(plate, pos, duration);
        protected WorldMap map => hidePool.map;
        protected Pos prevPos => hidePool.prevPos;
        protected HidePlate[,] plateData => hidePool.plateData;

        protected int width;
        protected int height;
        protected Pos playerOffsetPos;

        protected HidePlateUpdater(HidePool hidePool, int width, int height)
        {
            this.hidePool = hidePool;

            int maxRange = 2 * Mathf.Max(width, height) - Mathf.Min(width, height) - 1;
            hidePool.plateData = new HidePlate[map.Width + maxRange, map.Height + maxRange];

            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Light weight simple version of RedrawRange() method
        /// </summary>
        public void InitRange(Pos playerPos, Plate[,] plateMap)
        {
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    int x = playerPos.x + i;
                    int y = playerPos.y + j;
                    plateData[x, y] = GetInstance(plateMap[i, j], OffsetPos(playerPos, i, j));
                }
            }
        }

        public void RedrawRange(Pos playerPos, Plate[,] plateMap, int xShrink = 0, int yShrink = 0)
        {
            for (int j = yShrink; j < height - yShrink; j++)
            {
                for (int i = xShrink; i < width - xShrink; i++)
                {
                    int x = playerPos.x + i;
                    int y = playerPos.y + j;

                    if (plateMap[i, j] != (plateData[x, y] == null ? Plate.NONE : plateData[x, y].plate))
                    {
                        plateData[x, y]?.Remove(0.25f);
                        plateData[x, y] = GetInstance(plateMap[i, j], OffsetPos(playerPos, i, j), 0.25f);
                    }
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
            for (int i = 0; i < width; i++)
            {
                plateData[playerPos.x + i, playerPos.y + height]?.Remove();
                plateData[playerPos.x + i, playerPos.y] = GetInstance(plateMap[i, 0], OffsetPos(playerPos, i, 0));
            }
            RedrawYShrink(playerPos, plateMap);
        }
        private void MoveRangeSouth(Pos playerPos, Plate[,] plateMap)
        {
            for (int i = 0; i < width; i++)
            {
                plateData[playerPos.x + i, playerPos.y - 1]?.Remove();
                plateData[playerPos.x + i, playerPos.y + height - 1] = GetInstance(plateMap[i, height - 1], OffsetPos(playerPos, i, height - 1));
            }
            RedrawYShrink(playerPos, plateMap);
        }
        private void MoveRangeWest(Pos playerPos, Plate[,] plateMap)
        {
            for (int j = 0; j < height; j++)
            {
                plateData[playerPos.x + width, playerPos.y + j]?.Remove();
                plateData[playerPos.x, playerPos.y + j] = GetInstance(plateMap[0, j], OffsetPos(playerPos, 0, j));
            }
            RedrawXShrink(playerPos, plateMap);
        }
        private void MoveRangeEast(Pos playerPos, Plate[,] plateMap)
        {
            for (int j = 0; j < height; j++)
            {
                plateData[playerPos.x - 1, playerPos.y + j]?.Remove();
                plateData[playerPos.x + width - 1, playerPos.y + j] = GetInstance(plateMap[width - 1, j], OffsetPos(playerPos, width - 1, j));
            }
            RedrawXShrink(playerPos, plateMap);
        }

        private Pos OffsetPos(Pos pos, int offsetX, int offsetY)
            => new Pos(pos.x - width / 2 + offsetX, pos.y - height / 2 + offsetY);

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

            var openStack = new Stack<(int x, int y)>();

            // Start from player tile
            for (openStack.Push((width / 2, height / 2)); openStack.Count > 0;)
            {
                var pos = openStack.Pop();

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
                    openStack.Push((pos.x - 1, pos.y));
                }
                else if (pos.x - 1 >= 0)
                {
                    plateMap[pos.x - 1, pos.y] &= ~Plate.BD;
                }

                if (viewOpen[ptX, ptY - 1])
                {
                    openStack.Push((pos.x, pos.y - 1));
                }
                else if (pos.y - 1 >= 0)
                {
                    plateMap[pos.x, pos.y - 1] &= ~Plate.CD;
                }

                if (viewOpen[ptX + 1, ptY])
                {
                    openStack.Push((pos.x + 1, pos.y));
                }
                else if (pos.x + 1 < width)
                {
                    plateMap[pos.x + 1, pos.y] &= ~Plate.AC;
                }

                if (viewOpen[ptX, ptY + 1])
                {
                    openStack.Push((pos.x, pos.y + 1));
                }
                else if (pos.y + 1 < height)
                {
                    plateMap[pos.x, pos.y + 1] &= ~Plate.AB;
                }
            }

            return plateMap;
        }
    }
    protected class LandscapeUpdater : HidePlateUpdater
    {
        public LandscapeUpdater(HidePool hidePool, int range) : base(hidePool, range, range)
        {
            playerOffsetPos = new Pos(range / 2, range / 2);
        }
    }

}
