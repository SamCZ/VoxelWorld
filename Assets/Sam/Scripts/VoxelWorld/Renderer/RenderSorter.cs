using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderSorter : IComparer<Vector3Int>
{
    public float distanceToCamera(Vector3Int vec)
    {
        return Vector3.Distance(
            new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z),

            new Vector3(vec.x * ChunkRenderer.CHUNK_SIZE + 8, vec.y * ChunkRenderer.CHUNK_SIZE + 8, vec.z * ChunkRenderer.CHUNK_SIZE + 8));
    }

    public int Compare(Vector3Int left, Vector3Int right)
    {
        float pos1 = distanceToCamera(left);
        float pos2 = distanceToCamera(right);
        return pos1 < pos2 ? -1 : (pos1 > pos2 ? 1 : -1);
    }

}
