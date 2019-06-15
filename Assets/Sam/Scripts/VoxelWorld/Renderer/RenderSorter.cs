using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderSorter : IComparer<Vector2Int> {

    public float distanceToCamera(Vector2Int vec) {
        return Vector3.Distance(new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z), new Vector3(vec.x * ChunkRenderer.CHUNK_SIZE + 8, 0, vec.y * ChunkRenderer.CHUNK_SIZE + 8));
    }

    public int Compare(Vector2Int x, Vector2Int y) {
        float pos1 = distanceToCamera(x);
        float pos2 = distanceToCamera(y);
        return pos1 < pos2 ? -1 : (pos1 > pos2 ? 1 : -1);
    }

}
