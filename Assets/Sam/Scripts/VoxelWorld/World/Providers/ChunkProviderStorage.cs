using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkProviderStorage : IChunkProvider
{

    private IChunkProvider chunkGenerator;
    private WorldSaver worldSaver;
    private Dictionary<Vector2Int, Chunk> loadedChunks;

    public ChunkProviderStorage(IChunkProvider chunkGenerator, WorldSaver worldSaver)
    {
        this.chunkGenerator = chunkGenerator;
        this.worldSaver = worldSaver;
        this.loadedChunks = new Dictionary<Vector2Int, Chunk>();
    }

    public Chunk getChunk(int x, int z)
    {
        Vector2Int pos = new Vector2Int(x, z);
        Chunk chunk = null;
        //TODO: Manage storage memory
        if (this.loadedChunks.TryGetValue(pos, out chunk))
        {
            return chunk;
        }
        else
        {
            chunk = this.chunkGenerator.getChunk(x, z);
            Dictionary<Vector3Int, byte> mbs = this.worldSaver.getSavedBlocks(x, z);
            if (mbs != null && false)
            {
                foreach (KeyValuePair<Vector3Int, byte> mb in mbs)
                {
                    chunk.setBlock(mb.Key.x, mb.Key.y, mb.Key.z, mb.Value);
                }
            }
            chunk.updateHeightMap();
            this.loadedChunks.Add(pos, chunk);
        }
        return chunk;
    }

    public void onBlockUpdate(int x, int y, int z, byte blockId)
    {
        this.worldSaver.onBlockUpdate(x, y, z, blockId);
    }
}
