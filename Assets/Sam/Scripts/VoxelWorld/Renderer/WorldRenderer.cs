using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRenderer : IWorldAccess {

    private World world;
    private GameObject parentGameObject;
    private List<Block> blockDefines;
    private Material[] materials;

    private Dictionary<Vector2Int, ChunkRenderer> activeRenderers = new Dictionary<Vector2Int, ChunkRenderer>();
    private List<ChunkRenderer> freeRenderers = new List<ChunkRenderer>();
    private List<Vector2Int> locationsToFill = new List<Vector2Int>();

    private List<Vector2Int> rendererToRemove = new List<Vector2Int>();

    private RenderSorter sorter;
    private float prevSortX;
    private float prevSortZ;
    private bool fistUpdate;

    private Vector3 lastCameraRotation;
    private Plane[] planes;

    public WorldRenderer(World world, List<Block> blockDefines, GameObject gameObject, Material[] materials) {
        this.world = world;
        this.blockDefines = blockDefines;
        this.parentGameObject = gameObject;
        this.materials = materials;
        this.sorter = new RenderSorter();
    }

    public void update(int drawDistance) {
        Vector3 camRotation = Camera.main.transform.eulerAngles;
        if(this.lastCameraRotation == null || (this.lastCameraRotation != null && this.lastCameraRotation != camRotation)) {
            this.lastCameraRotation = camRotation;
            this.planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        }

        int renderersCount = this.activeRenderers.Count + this.freeRenderers.Count;
        if (renderersCount < Mathf.Pow(drawDistance+1, 4)) {
            for (int i = 0; i < (Mathf.Pow(drawDistance + 1, 4) - renderersCount); i++) {
                this.freeRenderers.Add(new ChunkRenderer(0, 0, this.world, this.blockDefines, this.parentGameObject, this.materials));
            }
        }

        Vector3 camLocation = Camera.main.transform.position;
        
        float playerX = camLocation.x;
        float playerZ = camLocation.z;

        float d1 = playerX - this.prevSortX;
        float d2 = playerZ - this.prevSortZ;

        if (d1 * d1 + d2 * d2 > 16F || !this.fistUpdate) {
            this.fistUpdate = true;
            this.prevSortX = playerX;
            this.prevSortZ = playerZ;

            int px = Mathf.FloorToInt(camLocation.x / 16);
            int pz = Mathf.FloorToInt(camLocation.z / 16);

            this.locationsToFill.Clear();

            for (int cx = px - drawDistance; cx < px + drawDistance; cx++) {
                for (int cz = pz - drawDistance; cz < pz + drawDistance; cz++) {
                    Vector2Int loc = new Vector2Int(cx, cz);
                    if (!this.locationsToFill.Contains(loc) && !this.activeRenderers.ContainsKey(loc)) {
                        this.locationsToFill.Add(loc);
                    }
                }
            }

            this.locationsToFill.Sort(this.sorter);
        }

        this.rendererToRemove.Clear();
        foreach (KeyValuePair<Vector2Int, ChunkRenderer> e in this.activeRenderers) {
            if(e.Value.checkDistance(camLocation, drawDistance)) {
                e.Value.disableObject();
                this.rendererToRemove.Add(e.Key);
            }
        }
        foreach(Vector2Int l in this.rendererToRemove) {
            ChunkRenderer r = this.activeRenderers[l];
            this.activeRenderers.Remove(l);
            this.freeRenderers.Add(r);
        }

        for (int i = 0; i < this.locationsToFill.Count; i++) {
            if (this.freeRenderers.Count > 0 && this.locationsToFill.Count > 0) {
                Vector2Int pos = this.locationsToFill[i];
                ChunkRenderer renderer = this.freeRenderers[0];
                renderer.setLocation(pos.x, pos.y);
                if(renderer.isInFrustum(this.planes) || renderer.isPlayerStandingOn(camLocation)) {
                    this.locationsToFill.RemoveAt(i);
                    this.freeRenderers.RemoveAt(0);
                    this.activeRenderers.Add(pos, renderer);
                    break;
                }
            }
        }

        foreach(ChunkRenderer r in this.activeRenderers.Values) {
            r.isInFrustum(this.planes);
            r.update(camLocation);
        }
    }

    public void onBlockUpdate(int x, int y, int z) {
        int cx = x >> 4;
        int cz = z >> 4;

        int bx = x & 15;
        int bz = z & 15;

        List<Vector2Int> dirty = new List<Vector2Int>();
        dirty.Add(new Vector2Int(cx, cz));

        if(bx == 0) {
            dirty.Add(new Vector2Int(cx - 1, cz));
        }
        if (bx == 15) {
            dirty.Add(new Vector2Int(cx + 1, cz));
        }
        if (bz == 0) {
            dirty.Add(new Vector2Int(cx, cz - 1));
        }
        if (bz == 15) {
            dirty.Add(new Vector2Int(cx, cz + 1));
        }

        foreach (Vector2Int l in dirty) {
            this.activeRenderers[l].markDirty();
        }
    }
}
