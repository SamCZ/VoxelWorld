using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World {

    private ChunkProviderStorage chunkProvider;
    private IWorldAccess worldAceess;

    public World(IChunkProvider chunkGenerator, WorldSaver worldSaver) {
        this.chunkProvider = new ChunkProviderStorage(chunkGenerator, worldSaver);
    }

    public byte getBlockId(int x, int y, int z) {
        Chunk chunk = getChunkFromWorldCoords(x, z);
        return chunk.getBlockId(x & 15, y, z & 15);
    }

    public void setBlockId(int x, int y, int z, byte blockId, bool update = true) {
        Chunk chunk = getChunkFromWorldCoords(x, z);
        chunk.setBlock(x & 15, y, z & 15, blockId);
        if (update && this.worldAceess != null) {
            this.worldAceess.onBlockUpdate(x, y, z);
        }
        this.chunkProvider.onBlockUpdate(x, y, z, blockId);
    }

    public void setBlockId(Vector3 position, byte blockId, bool update = true) {
        this.setBlockId((int)position.x, (int)position.y, (int)position.z, blockId, update);
    }

    public int getHeight(int x, int z) {
        Chunk chunk = getChunkFromWorldCoords(x, z);
        return chunk.getMaxHeight(x & 15, z & 15);
    }

    public Chunk getChunk(int x, int z) {
        return this.chunkProvider.getChunk(x, z);
    }

    public Chunk getChunkFromWorldCoords(int x, int z) {
        return getChunk(x >> 4, z >> 4);
    }

    public void setWorldAccess(IWorldAccess worldAceess) {
        this.worldAceess = worldAceess;
    }
	
}
