using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VoxelMeshType
{
    Block,
    Cross
}

public class ChunkRenderer
{

    public static int CHUNK_SIZE = 16;

    private int chunkX;
    private int chunkY;
    private int chunkZ;
    private World world;
    private List<Block> blockDefines;
    private Material[] materials;

    private GameObject parentGameObject;
    private GameObject obj;
    private GameObject[] objData;
    private Bounds boundingBox;
    private bool isNeedUpdate;
    private bool isVisible;
    private bool lastVisibilityState;
    private bool isEnabled;

    private ChunkRenderCache chunkRenderCache;

    public ChunkRenderer(int chunkX, int chunkY, int chunkZ, World world, List<Block> blockDefines, GameObject gameObject, Material[] materials)
    {
        this.chunkX = chunkX;
        this.chunkY = chunkY;
        this.chunkZ = chunkZ;
        this.world = world;
        this.blockDefines = blockDefines;
        this.parentGameObject = gameObject;
        this.materials = materials;
        this.isNeedUpdate = true;
        this.objData = new GameObject[2];
        updateObject();
        this.updateBounds();
    }

    private void updateBounds()
    {
        this.boundingBox = new Bounds(new Vector3(this.chunkX * CHUNK_SIZE + 8, this.chunkY * CHUNK_SIZE + 8, this.chunkZ * CHUNK_SIZE + 8), new Vector3(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE));
    }

    private void updateObject()
    {
        if (obj == null)
        {
            obj = new GameObject(this.ToString());
            obj.transform.parent = this.parentGameObject.transform;
        }
        else
        {
            obj.name = this.ToString();
        }
        obj.transform.position = new Vector3(this.chunkX * CHUNK_SIZE, this.chunkY * CHUNK_SIZE, this.chunkZ * CHUNK_SIZE);
        this.updateBounds();
    }

    public void setLocation(int x, int y, int z)
    {
        if (x != this.chunkX || y != this.chunkY || z != this.chunkZ)
        {
            this.chunkX = x;
            this.chunkY = y;
            this.chunkZ = z;

            chunkRenderCache = new ChunkRenderCache(world, x, y, z);

            this.isNeedUpdate = true;
            updateObject();
        }
    }

    public void setEnabled(bool enabled)
    {
        if (!isEnabled && !enabled)
        {
            // If last state was disabled, dont disable again disabled object.
            return;
        }

        isEnabled = enabled;

        this.obj.SetActive(enabled);
    }

    public bool isPlayerStandingOn(Vector3 position)
    {
        return (Mathf.FloorToInt(position.x) >> 4 == this.chunkX) && (Mathf.FloorToInt(position.z) >> 4 == this.chunkZ);
    }

    private GameObject prepareForUpload(bool transparent, bool empty)
    {
        int index = transparent ? 1 : 0;
        if (this.objData[index] != null && empty)
        {
            this.objData[index].SetActive(false);
            return null;
        }
        else
        {
            if (empty) return null;
            GameObject obj = this.objData[index];
            if (obj == null)
            {
                obj = new GameObject(transparent ? "Transparent" : "Opaque");
                obj.transform.parent = this.obj.transform;
                obj.transform.localPosition = new Vector3(0, 0, 0);
                this.objData[index] = obj;
            }
            obj.SetActive(true);
            setEnabled(true);
            return obj;
        }
    }

    private byte getBlockId(int x, int y, int z)
    {
        return chunkRenderCache.getBlockId(x, y, z);
    }

    private Block getBlock(int x, int y, int z)
    {
        return getBlock(getBlockId(x, y, z));
    }

    private Block getBlock(byte blockId)
    {
        if (blockId - 1 < this.blockDefines.Count && blockId - 1 > 0)
        {
            return this.blockDefines[blockId - 1];
        }
        return Block.AIR;
    }

    public void update(Vector3 player)
    {
        if (this.lastVisibilityState != this.isVisible)
        {
            this.lastVisibilityState = this.isVisible;
            setEnabled(this.isVisible ? true : this.isPlayerStandingOn(player));
        }

        if (!this.isNeedUpdate) return;
        this.isNeedUpdate = false;

        MeshBuilder opaqueBuilder = MeshBuilder.OPAQUE_BUILDER;
        MeshBuilder transparentBuilder = MeshBuilder.TRANSPARENT_BUILDER;

        opaqueBuilder.clear();
        transparentBuilder.clear();

        byte topBlock = 0;
        byte bottomBlock = 0;
        byte leftBlock = 0;
        byte rightBlock = 0;
        byte backBlock = 0;
        byte frontBlock = 0;

        Chunk chunk = this.world.getChunk(this.chunkX, this.chunkZ);

        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                for (int y = 0; y < 16; y++)
                {
                    byte blockId = getBlockId(x, y, z);
                    if (blockId - 1 < this.blockDefines.Count && blockId > 0)
                    {
                        Block block = this.blockDefines[blockId - 1];

                        if (block.meshType == VoxelMeshType.Block)
                        {
                            topBlock = getBlockId(x, y + 1, z);
                            bottomBlock = getBlockId(x, y - 1, z);
                            leftBlock = getBlockId(x - 1, y, z);
                            rightBlock = getBlockId(x + 1, y, z);
                            backBlock = getBlockId(x, y, z + 1);
                            frontBlock = getBlockId(x, y, z - 1);

                            if (getBlock(topBlock).isTransparent)
                            {
                                if (topBlock != blockId || block.renderAllFaces) topBlock = 0;
                            }

                            if (getBlock(bottomBlock).isTransparent)
                            {
                                if (bottomBlock != blockId || block.renderAllFaces) bottomBlock = 0;
                            }

                            if (getBlock(backBlock).isTransparent)
                            {
                                if (backBlock != blockId || block.renderAllFaces) backBlock = 0;
                            }

                            if (getBlock(frontBlock).isTransparent)
                            {
                                if (frontBlock != blockId || block.renderAllFaces) frontBlock = 0;
                            }

                            if (getBlock(leftBlock).isTransparent)
                            {
                                if (leftBlock != blockId || block.renderAllFaces) leftBlock = 0;
                            }

                            if (getBlock(rightBlock).isTransparent)
                            {
                                if (rightBlock != blockId || block.renderAllFaces) rightBlock = 0;
                            }

                            if (topBlock > 0 && bottomBlock > 0 && backBlock > 0 && frontBlock > 0 && leftBlock > 0 && rightBlock > 0)
                            {
                                continue;
                            }
                        }

                        float microspace = 0.00001f;

                        float size = 1.0f + microspace;
                        float sizeY = block.height + microspace;

                        if (block.meshType == VoxelMeshType.Cross)
                        {
                            this.addBoxFace(block, BlockFace.Left,
                                        new float[] { x, y + sizeY, z },
                                        new float[] { x, y, z },
                                        new float[] { x + size, y, z + size },
                                        new float[] { x + size, y + sizeY, z + size });

                            this.addBoxFace(block, BlockFace.Right,
                                        new float[] { x + size, y + sizeY, z + size },
                                        new float[] { x + size, y, z + size },
                                        new float[] { x + 0, y, z },
                                        new float[] { x + 0, y + sizeY, z });


                            this.addBoxFace(block, BlockFace.Front,
                                        new float[] { x + size, y + sizeY, z },
                                        new float[] { x + size, y, z },
                                        new float[] { x, y, z + size },
                                        new float[] { x, y + sizeY, z + size });

                            this.addBoxFace(block, BlockFace.Back,
                                        new float[] { x, y + sizeY, z + size },
                                        new float[] { x, y, z + size },
                                        new float[] { x + size, y, z + 0 },
                                        new float[] { x + size, y + sizeY, z + 0 });
                        }
                        else
                        {
                            if (topBlock == 0)
                            {
                                this.addBoxFace(block, BlockFace.Top,
                                    new float[] { x, y + sizeY, z },
                                    new float[] { x, y + sizeY, z + size },
                                    new float[] { x + size, y + sizeY, z + size },
                                    new float[] { x + size, y + sizeY, z });
                            }

                            if (bottomBlock == 0)
                            {
                                this.addBoxFace(block, BlockFace.Bottom,
                                        new float[] { x, y, z },
                                        new float[] { x + sizeY, y, z },
                                        new float[] { x + sizeY, y, z + size },
                                        new float[] { x, y, z + size });
                            }

                            if (leftBlock == 0)
                            {
                                this.addBoxFace(block, BlockFace.Left,
                                        new float[] { x, y + sizeY, z },
                                        new float[] { x, y, z },
                                        new float[] { x, y, z + size },
                                        new float[] { x, y + sizeY, z + size });
                            }

                            if (rightBlock == 0)
                            {
                                this.addBoxFace(block, BlockFace.Right,
                                        new float[] { x + size, y + sizeY, z + size },
                                        new float[] { x + size, y, z + size },
                                        new float[] { x + size, y, z },
                                        new float[] { x + size, y + sizeY, z });
                            }

                            if (frontBlock == 0)
                            {
                                this.addBoxFace(block, BlockFace.Front,
                                        new float[] { x + size, y + sizeY, z },
                                        new float[] { x + size, y, z },
                                        new float[] { x, y, z },
                                        new float[] { x, y + sizeY, z });
                            }

                            if (backBlock == 0)
                            {
                                this.addBoxFace(block, BlockFace.Back,
                                        new float[] { x, y + sizeY, z + size },
                                        new float[] { x, y, z + size },
                                        new float[] { x + size, y, z + size },
                                        new float[] { x + size, y + sizeY, z + size });
                            }
                        }
                    }
                }
            }
        }

        opaqueBuilder.UploadData(prepareForUpload(false, opaqueBuilder.isEmpty()), this.materials[0]);
        transparentBuilder.UploadData(prepareForUpload(true, transparentBuilder.isEmpty()), this.materials[1]);
    }

    private void addBoxFace(Block block, BlockFace side, float[] par1, float[] par2, float[] par3, float[] par4)
    {
        MeshBuilder builder = block.isTransparent ? MeshBuilder.TRANSPARENT_BUILDER : MeshBuilder.OPAQUE_BUILDER;

        builder.addQuad(par1, par2, par3, par4, block.isSolid);

        float tUnit = 1F / (4096.0f / 1024.0f);

        Vector2 texture = block.faceTexturePos[block.faceTexturePos.Length > 1 ? (int)side : 0];

        builder.addUV(tUnit * texture.x, -(tUnit * texture.y));
        builder.addUV(tUnit * texture.x, -(tUnit * texture.y + tUnit));
        builder.addUV(tUnit * texture.x + tUnit, -(tUnit * texture.y + tUnit));
        builder.addUV(tUnit * texture.x + tUnit, -(tUnit * texture.y));
    }

    public bool checkDistance(Vector3 location, int drawDistance)
    {
        Vector3 thisLocation = new Vector3(this.chunkX, 0, this.chunkZ);
        Vector3 other = new Vector3(Mathf.FloorToInt(location.x / CHUNK_SIZE), 0, Mathf.FloorToInt(location.z / CHUNK_SIZE));

        return Vector3.Distance(thisLocation, other) > drawDistance + 3;
    }

    public bool isInFrustum(Plane[] planes)
    {
        if (this.boundingBox == null)
        {
            this.updateBounds();
        }
        return (this.isVisible = GeometryUtility.TestPlanesAABB(planes, this.boundingBox));
    }

    public bool isNeedsUpdate()
    {
        return this.isNeedUpdate;
    }

    public void markDirty()
    {
        this.isNeedUpdate = true;
    }

    public override string ToString()
    {
        return "Chunk(x=" + this.chunkX + ",y=" + this.chunkY + ",z=" + this.chunkZ + ")";
    }
}
