using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRenderer : IWorldAccess
{

    private World world;
    private GameObject parentGameObject;
    private List<Block> blockDefines;
    private Material[] materials;

    private Dictionary<Vector3Int, ChunkRenderer> activeRenderers = new Dictionary<Vector3Int, ChunkRenderer>();
    private List<ChunkRenderer> freeRenderers = new List<ChunkRenderer>();
    private List<Vector3Int> locationsToFill = new List<Vector3Int>();

    private List<Vector3Int> rendererToRemove = new List<Vector3Int>();

    private RenderSorter sorter;
    private float prevSortX;
    private float prevSortY;
    private float prevSortZ;
    private bool fistUpdate;

    private Vector3 lastCameraRotation;
    private Plane[] planes;

    public WorldRenderer(World world, List<Block> blockDefines, GameObject gameObject, Material[] materials)
    {
        this.world = world;
        this.blockDefines = blockDefines;
        this.parentGameObject = gameObject;
        this.materials = materials;
        this.sorter = new RenderSorter();
    }

    public void update(int drawDistance)
    {
        Vector3 camRotation = Camera.main.transform.eulerAngles;
        if (this.freeRenderers.Count == 0 || (this.lastCameraRotation != null && this.lastCameraRotation != camRotation))
        {
            this.lastCameraRotation = camRotation;
            this.planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        }

        int renderersCount = this.activeRenderers.Count + this.freeRenderers.Count;
        if (renderersCount < Mathf.Pow(drawDistance + 1, 4))
        {
            for (int i = 0; i < (Mathf.Pow(drawDistance + 1, 4) - renderersCount); i++)
            {
                for (int h = 0; h < 1; h++)
                {
                    this.freeRenderers.Add(new ChunkRenderer(0, 0, 0, this.world, this.blockDefines, this.parentGameObject, this.materials));
                }
            }
        }

        Vector3 camLocation = Camera.main.transform.position;

        float playerX = camLocation.x;
        float playerY = camLocation.y;
        float playerZ = camLocation.z;

        float d1 = playerX - this.prevSortX;
        float d2 = playerY - this.prevSortY;
        float d3 = playerZ - this.prevSortZ;

        if (d1 * d1 + d2 * d2 + d3 * d3 > 16F || !this.fistUpdate)
        {
            this.fistUpdate = true;
            this.prevSortX = playerX;
            this.prevSortY = playerY;
            this.prevSortZ = playerZ;

            int px = Mathf.FloorToInt(playerX / 16);
            int py = Mathf.FloorToInt(playerY / 16);
            int pz = Mathf.FloorToInt(playerZ / 16);

            this.locationsToFill.Clear();

            Vector3Int loc = new Vector3Int();

            for (int cx = px - drawDistance; cx < px + drawDistance; cx++)
            {
                for (int cz = pz - drawDistance; cz < pz + drawDistance; cz++)
                {
                    for (int cy = py - drawDistance; cy < py + drawDistance; cy++)
                    {
                        loc.x = cx;
                        loc.y = cy;
                        loc.z = cz;

                        if (!this.activeRenderers.ContainsKey(loc))
                        {
                            this.locationsToFill.Add(loc);
                        }
                    }
                }
            }

            this.locationsToFill.Sort(this.sorter);
        }

        this.rendererToRemove.Clear();
        foreach (KeyValuePair<Vector3Int, ChunkRenderer> e in this.activeRenderers)
        {
            if (e.Value.checkDistance(camLocation, drawDistance))
            {
                e.Value.setEnabled(false);
                this.rendererToRemove.Add(e.Key);
            }
        }
        foreach (Vector3Int l in this.rendererToRemove)
        {
            ChunkRenderer r = this.activeRenderers[l];
            this.activeRenderers.Remove(l);
            this.freeRenderers.Add(r);
        }

        for (int i = 0; i < this.locationsToFill.Count; i++)
        {
            if (this.freeRenderers.Count > 0 && this.locationsToFill.Count > 0)
            {
                Vector3Int pos = this.locationsToFill[i];
                ChunkRenderer renderer = this.freeRenderers[0];
                renderer.setLocation(pos.x, pos.y, pos.z);
                if (renderer.isInFrustum(this.planes) || renderer.isPlayerStandingOn(camLocation))
                {
                    this.locationsToFill.RemoveAt(i);
                    this.freeRenderers.RemoveAt(0);
                    this.activeRenderers.Add(pos, renderer);
                    break;
                }
            }
        }

        foreach (ChunkRenderer r in this.activeRenderers.Values)
        {
            r.isInFrustum(this.planes);
            r.update(camLocation);
        }
    }

    public void onBlockUpdate(int x, int y, int z)
    {
        int cx = x >> 4;
        int cy = y >> 4;
        int cz = z >> 4;

        int bx = x & 15;
        int by = y & 15;
        int bz = z & 15;

        List<Vector3Int> dirty = new List<Vector3Int>();
        dirty.Add(new Vector3Int(cx, cy, cz));

        if (bx == 0)
        {
            dirty.Add(new Vector3Int(cx - 1, cy, cz));
        }
        if (bx == 15)
        {
            dirty.Add(new Vector3Int(cx + 1, cy, cz));
        }

        if (by == 0)
        {
            dirty.Add(new Vector3Int(cx, cy - 1, cz));
        }

        if (by == 15)
        {
            dirty.Add(new Vector3Int(cx, cy + 1, cz));
        }

        if (bz == 0)
        {
            dirty.Add(new Vector3Int(cx, cy, cz - 1));
        }
        if (bz == 15)
        {
            dirty.Add(new Vector3Int(cx, cy, cz + 1));
        }

        foreach (Vector3Int l in dirty)
        {
            this.activeRenderers[l].markDirty();
        }
    }
}
