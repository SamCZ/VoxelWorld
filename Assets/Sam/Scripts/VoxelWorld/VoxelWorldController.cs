using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldGenerationType {
    Noise, Flat
}

public class VoxelWorldController : MonoBehaviour {

    public Material opaqueMaterial;
    public Material transparentMaterial;
    public int textureSize = 512;
    public int tileSize = 16;
    public WorldGenerationType generationType;
    [Range(1, 200)]
    public int drawDistance = 5;

    public int debugDrawLimit = 5;
    
    [HideInInspector]
    public List<Block> blocks;

    [HideInInspector]
    public WorldSaver worldSaver = new WorldSaver("Assets/Save01");
    private World world;
    private WorldRenderer worldRenderer;

    private bool isBeenSpawned = false;

    void Start () {
        IChunkProvider chunkGenerator = null;
        if(this.generationType == WorldGenerationType.Flat) {
            chunkGenerator = new ChunkProviderGenratorFlat();
        } else if(this.generationType == WorldGenerationType.Noise) {
            chunkGenerator = new ChunkProviderGeneratorNoise(0, this.blocks);
        }
        this.world = new World(chunkGenerator, this.worldSaver);
        this.worldRenderer = new WorldRenderer(this.world, this.blocks, this.gameObject, new Material[] { this.opaqueMaterial, this.transparentMaterial });
        this.world.setWorldAccess(this.worldRenderer);
	}

    void OnDisable() {
        this.worldSaver.savePlayerLocation(Camera.main.transform);
        this.worldSaver.saveAll();
    }

    private void randomSpawn() {
        float x = (Random.value * 2.0f - 1.0f) * 60;
        float z = (Random.value * 2.0f - 1.0f) * 60;

        int bx = Mathf.FloorToInt(x);
        int bz = Mathf.FloorToInt(z);

        int maxHeight = this.world.getHeight(bx, bz);
        Camera.main.transform.parent.position = new Vector3(x + 0.5f, maxHeight + 2.5f, z + 0.5f);
    }

    void Update () {
        if (!this.isBeenSpawned) {
            if(!this.worldSaver.loadPlayerLocation(Camera.main.transform)) {
                this.randomSpawn();
            }
            this.isBeenSpawned = true;
        }
        this.worldRenderer.update(this.drawDistance);
	}

    public bool getPlayerBlockPick(Transform transform, out Vector3 lookingAt, out Vector3 placeAt, out byte blockId, int pickBlockDistance = 8) {
        lookingAt = Vector3.zero;
        placeAt = Vector3.zero;
        blockId = 0;

        float xn = transform.position.x;
        float yn = transform.position.y;
        float zn = transform.position.z;

        float xl;
        float yl;
        float zl;

        float yChange = (float)Mathf.Cos((transform.eulerAngles.x + 90) / 180 * Mathf.PI);
        float ymult = (float)Mathf.Sin((transform.eulerAngles.x + 90) / 180 * Mathf.PI);

        float xChange = (float)(-Mathf.Cos((transform.eulerAngles.y + 90) / 180 * Mathf.PI) * ymult);
        float zChange = (float)(Mathf.Sin((transform.eulerAngles.y + 90) / 180 * Mathf.PI) * ymult);

        for (float f = 0; f <= pickBlockDistance; f += 0.01f) {
            xl = xn;
            yl = yn;
            zl = zn;

            xn = transform.position.x + f * xChange;
            yn = transform.position.y + f * yChange;
            zn = transform.position.z + f * zChange;

            blockId = this.world.getBlockId(Mathf.FloorToInt(xn), Mathf.FloorToInt(yn), Mathf.FloorToInt(zn));
            if (blockId > 0) {
                lookingAt = new Vector3(Mathf.FloorToInt(xn), Mathf.FloorToInt(yn), Mathf.FloorToInt(zn));
                placeAt = new Vector3(Mathf.FloorToInt(xl), Mathf.FloorToInt(yl), Mathf.FloorToInt(zl));
                return true;
            }
        }
        return false;
    }

    public World getWorld() {
        return this.world;
    }

    public Block getBlockInstance(byte blockId) {
        if (blockId - 1 < this.blocks.Count && blockId - 1 >= 0) {
            return this.blocks[blockId - 1];
        }
        return Block.AIR;
    }

    public List<Block> getBlocks() {
        return this.blocks;
    }
}
