using UnityEngine;
using System;
using System.Collections.Generic;

public class HidePool
{
    protected Transform[] pool = new Transform[0b10000];
    protected GameObject[] prefab = new GameObject[0b10000];
    protected Quaternion[] rotate = new Quaternion[0b10000];

    protected WorldMap map;

    protected const int RANGE = 11;
    protected HidePlate[,] plateData;
    protected Pos prevPos = new Pos();

    public HidePool(WorldMap map, GameObject plate5, GameObject plate4, GameObject plate3, GameObject plate2, GameObject plate1)
    {
        this.map = map;
        this.plateData = new HidePlate[RANGE + map.Width, RANGE + map.Height];

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

    public HidePlate GetInstance(Plate plate, Pos pos, float duration = 0.01f)
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

    public void UpdateHidePlates(Vector3 playerPos, Action<Pos, Plate[,]> drawAction)
    {
        Pos mapPos = map.MapPos(playerPos);
        Plate[,] plateMap = GetPlateMap(mapPos);

        drawAction(mapPos, plateMap);

        prevPos = mapPos;
    }

    public void RedrawHidePlates(Vector3 playerPos)
    {
        UpdateHidePlates(playerPos, (mapPos, plateMap) =>
        {
            if (!prevPos.IsNull)
            {
                RedrawRange(mapPos, plateMap);
            }
            else
            {
                InitRange(mapPos, plateMap);
            }
        });
    }

    public void MoveHidePlates(Vector3 playerPos)
    {
        UpdateHidePlates(playerPos, (mapPos, plateMap) =>
        {
            if (!prevPos.IsNull)
            {
                Pos moveVec = mapPos - prevPos;

                if (moveVec.y < 0) MoveRangeNorth(plateMap, mapPos);
                if (moveVec.y > 0) MoveRangeSouth(plateMap, mapPos);
                if (moveVec.x < 0) MoveRangeWest(plateMap, mapPos);
                if (moveVec.x > 0) MoveRangeEast(plateMap, mapPos);
            }
        });
    }

    private void MoveRangeNorth(Plate[,] plateMap, Pos playerPos)
    {
        for (int i = 0; i < RANGE; i++)
        {
            plateData[playerPos.x + i, playerPos.y + RANGE]?.Remove();
            plateData[playerPos.x + i, playerPos.y] = GetInstance(plateMap[i, 0], OffsetPos(playerPos, i, 0));
        }
        RedrawRange(playerPos, plateMap, false, true);
    }
    private void MoveRangeSouth(Plate[,] plateMap, Pos playerPos)
    {
        for (int i = 0; i < RANGE; i++)
        {
            plateData[playerPos.x + i, playerPos.y - 1]?.Remove();
            plateData[playerPos.x + i, playerPos.y + RANGE - 1] = GetInstance(plateMap[i, RANGE - 1], OffsetPos(playerPos, i, RANGE - 1));
        }
        RedrawRange(playerPos, plateMap, false, true);
    }
    private void MoveRangeWest(Plate[,] plateMap, Pos playerPos)
    {
        for (int j = 0; j < RANGE; j++)
        {
            plateData[playerPos.x + RANGE, playerPos.y + j]?.Remove();
            plateData[playerPos.x, playerPos.y + j] = GetInstance(plateMap[0, j], OffsetPos(playerPos, 0, j));
        }
        RedrawRange(playerPos, plateMap, true, false);
    }
    private void MoveRangeEast(Plate[,] plateMap, Pos playerPos)
    {
        for (int j = 0; j < RANGE; j++)
        {
            plateData[playerPos.x - 1, playerPos.y + j]?.Remove();
            plateData[playerPos.x + RANGE - 1, playerPos.y + j] = GetInstance(plateMap[RANGE - 1, j], OffsetPos(playerPos, RANGE - 1, j));
        }
        RedrawRange(playerPos, plateMap, true, false);
    }

    private void InitRange(Pos playerPos, Plate[,] plateMap)
    {
        for (int j = 0; j < RANGE; j++)
        {
            for (int i = 0; i < RANGE; i++)
            {
                int x = playerPos.x + i;
                int y = playerPos.y + j;
                plateData[x, y] = GetInstance(plateMap[i, j], OffsetPos(playerPos, i, j));
            }
        }
    }

    private void RedrawRange(Pos playerPos, Plate[,] plateMap, bool isXShrink = false, bool isYShrink = false)
    {
        int xShrink = isXShrink ? 1 : 0;
        int yShrink = isYShrink ? 1 : 0;

        for (int j = yShrink; j < RANGE - yShrink; j++)
        {
            for (int i = xShrink; i < RANGE - xShrink; i++)
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

    private Pos OffsetPos(Pos pos, int offsetX, int offsetY)
        => new Pos(pos.x - RANGE / 2 + offsetX, pos.y - RANGE / 2 + offsetY);

    /// <summary>
    /// Get tiles inside player view range with see through info
    /// </summary>
    private bool[,] GetTileRegion(int x, int y)
    {
        int tileRange = RANGE + 2;
        var region = new bool[tileRange, tileRange];

        for (int i = 0; i < tileRange; i++)
        {
            region[i, 0] = region[i, tileRange - 1] = false;
        }

        for (int j = 1; j < tileRange - 1; j++)
        {
            region[0, j] = region[tileRange - 1, j] = false;
        }

        for (int j = 1; j < tileRange - 1; j++)
        {
            for (int i = 1; i < tileRange - 1; i++)
            {
                int matX = x - tileRange / 2 + i;
                int matY = y - tileRange / 2 + j;

                region[i, j] = map.IsTileViewOpen(matX, matY);
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
        // Get 13x13 player view range
        bool[,] region = GetTileRegion(playerPos.x, playerPos.y);

        var plateMap = new Plate[RANGE, RANGE];

        // Fill 11x11 tile with hide plates nearby player
        for (int j = 0; j < RANGE; j++)
        {
            for (int i = 0; i < RANGE; i++)
            {
                plateMap[i, j] = Plate.ABCD;
            }
        }

        var openStack = new Stack<(int x, int y)>();

        // Start from player tile
        for (openStack.Push((RANGE / 2, RANGE / 2)); openStack.Count > 0;)
        {
            var pos = openStack.Pop();

            // Delete focused hide plate
            plateMap[pos.x, pos.y] = Plate.NONE;

            // plateMap index[x, y] = region index[x+1, y+1]
            int ptX = pos.x + 1;
            int ptY = pos.y + 1;

            region[ptX, ptY] = false;

            // Delete corner of hide plates nearby focusing tile
            if (pos.x - 1 >= 0 && pos.y - 1 >= 0)
            {
                plateMap[pos.x - 1, pos.y - 1] &= ~Plate.D;
            }

            if (pos.x - 1 >= 0 && pos.y + 1 < RANGE)
            {
                plateMap[pos.x - 1, pos.y + 1] &= ~Plate.B;
            }

            if (pos.x + 1 < RANGE && pos.y - 1 >= 0)
            {
                plateMap[pos.x + 1, pos.y - 1] &= ~Plate.C;
            }

            if (pos.x + 1 < RANGE && pos.y + 1 < RANGE)
            {
                plateMap[pos.x + 1, pos.y + 1] &= ~Plate.A;
            }

            // Push neighbor tile as hide plate deleting candidate
            // If not a delete candidate, delete only a half of the plate
            if (region[ptX - 1, ptY])
            {
                openStack.Push((pos.x - 1, pos.y));
            }
            else if (pos.x - 1 >= 0)
            {
                plateMap[pos.x - 1, pos.y] &= ~Plate.BD;
            }

            if (region[ptX, ptY - 1])
            {
                openStack.Push((pos.x, pos.y - 1));
            }
            else if (pos.y - 1 >= 0)
            {
                plateMap[pos.x, pos.y - 1] &= ~Plate.CD;
            }

            if (region[ptX + 1, ptY])
            {
                openStack.Push((pos.x + 1, pos.y));
            }
            else if (pos.x + 1 < RANGE)
            {
                plateMap[pos.x + 1, pos.y] &= ~Plate.AC;
            }

            if (region[ptX, ptY + 1])
            {
                openStack.Push((pos.x, pos.y + 1));
            }
            else if (pos.y + 1 < RANGE)
            {
                plateMap[pos.x, pos.y + 1] &= ~Plate.AB;
            }
        }

        return plateMap;
    }
}
