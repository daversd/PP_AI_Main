﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[System.Serializable]
public class ConfigurablePart : Part
{
    public ConfigurablePart() { }

    public ConfigurablePart NewPart(VoxelGrid grid)
    {
        ConfigurablePart p = new ConfigurablePart();
        p.Type = PartType.Configurable;
        p.Orientation = (PartOrientation)System.Enum.Parse(typeof(PartOrientation), OrientationName, false);
        p._grid = grid;
        p.IsStatic = false;
        p.Height = Height;

        p.OCIndexes = OCIndexes;
        var indexes = p.OCIndexes.Split(';');
        p.nVoxels = indexes.Length;
        p.OccupiedIndexes = new Vector3Int[p.nVoxels];
        p.OccupiedVoxels = new Voxel[p.nVoxels];

        for (int i = 0; i < p.nVoxels; i++)
        {
            var index = indexes[i];
            var coords = index.Split('_');
            Vector3Int vector = new Vector3Int(int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2]));
            var voxel = grid.Voxels[vector.x, vector.y, vector.z];
            voxel.IsOccupied = true;
            voxel.Part = p;
            p.OccupiedIndexes[i] = vector;
            p.OccupiedVoxels[i] = voxel;

        }
        p.ReferenceIndex = p.OccupiedIndexes[0];
        p.CalculateCenter();
        return p;
    }


    public ConfigurablePart (VoxelGrid grid, List<Part> existingParts)
    {
        //This method creates a random configurable part in the specified grid. 
        //An overload is reequired to create a configurable part in a specific position
        Type = PartType.Configurable;
        _grid = grid;
        int minimumDistance = 6; //In voxels
        Size = new Vector2Int(6, 2); //6 x 2 configurable part size
        nVoxels = Size.x * Size.y;
        OccupiedIndexes = new Vector3Int[nVoxels];
        IsStatic = false;
        Height = 6;

        Random.InitState(5);
        bool validPart = false;
        while (!validPart)
        {
            Orientation = (PartOrientation)Random.Range(0, 2);
            int randomX = Random.Range(0, _grid.Size.x - 1);
            int randomY = Random.Range(0, _grid.Size.y - 1);
            int randomZ = Random.Range(0, _grid.Size.z - 1);
            ReferenceIndex = new Vector3Int(randomX, randomY, randomZ);

            bool allInside = true;

            GetOccupiedIndexes();
            if (!OnMinDistance(existingParts, minimumDistance)) continue;

            foreach (var index in OccupiedIndexes)
            {
                if (index.x >= _grid.Size.x || index.y >= _grid.Size.y || index.z >= _grid.Size.z)
                {
                    allInside = false;
                    break;
                }
                else if (_grid.Voxels[index.x, index.y, index.z].IsOccupied || !_grid.Voxels[index.x, index.y, index.z].IsActive)
                {
                    allInside = false;
                    break;
                }
            }
            if (allInside) validPart = true;
            else continue;
        }
        OccupyVoxels();
    }
    bool OnMinDistance(List<Part> existingParts, int minimumDistance)
    {
        if (existingParts.Count > 0)
        {
            foreach (var ePart in existingParts)
            {
                if (Orientation == ePart.Orientation)
                {
                    if (Orientation == PartOrientation.Horizontal)
                    {
                        foreach (var x in OccupiedIndexes.Select(i => i.x))
                        {
                            if (ePart.OccupiedIndexes.Any(e => e.x == x))
                            {
                                if (Mathf.Abs(ReferenceIndex.z - ePart.ReferenceIndex.z) <= minimumDistance) return false;
                            }
                        }
                    }
                    else if (Orientation == PartOrientation.Vertical)
                    {
                        foreach (var z in OccupiedIndexes.Select(i => i.z))
                        {
                            if (ePart.OccupiedIndexes.Any(ee => ee.z == z))
                            {
                                if (Mathf.Abs(ReferenceIndex.x - ePart.ReferenceIndex.x) <= minimumDistance) return false;
                            }
                        }

                    }
                }
            }
        }
        return true;
    }

    

    void GetOccupiedIndexes()
    {
        if (Orientation == PartOrientation.Horizontal)
        {
            int i = 0;
            for (int x = 0; x < Size.x; x++)
            {
                for (int z = 0; z < Size.y; z++)
                {
                    OccupiedIndexes[i++] = new Vector3Int(ReferenceIndex.x + x, ReferenceIndex.y, ReferenceIndex.z + z);
                }
            }

        }
        else if (Orientation == PartOrientation.Vertical)
        {
            int i = 0;
            for (int x = 0; x < Size.y; x++)
            {
                for (int z = 0; z < Size.x; z++)
                {
                    OccupiedIndexes[i++] = new Vector3Int(ReferenceIndex.x + x, ReferenceIndex.y, ReferenceIndex.z + z);
                }
            }
        }
    }
}
