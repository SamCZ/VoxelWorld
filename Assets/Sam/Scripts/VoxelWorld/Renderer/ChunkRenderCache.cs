
class ChunkRenderCache
{
    private static int CENTER = 0;
    private static int LEFT = 1;
    private static int RIGHT = 2;
    private static int FRONT = 3;
    private static int BACK = 4;

    private World m_World;
    private int m_ChunkX;
    private int m_ChunkY;
    private int m_ChunkZ;

    private Chunk[] m_CachedChunks;

    public ChunkRenderCache(World world, int chunkX, int chunkY, int chunkZ)
    {
        m_World = world;
        m_ChunkX = chunkX;
        m_ChunkY = chunkY;
        m_ChunkZ = chunkZ;

        m_CachedChunks = new Chunk[5];
        m_CachedChunks[CENTER] = world.getChunk(chunkX, chunkZ);
        m_CachedChunks[LEFT] = world.getChunk(chunkX - 1, chunkZ);
        m_CachedChunks[RIGHT] = world.getChunk(chunkX + 1, chunkZ);
        m_CachedChunks[FRONT] = world.getChunk(chunkX, chunkZ - 1);
        m_CachedChunks[BACK] = world.getChunk(chunkX, chunkZ + 1);
    }

    public byte getBlockId(int x, int y, int z)
    {
        Chunk chunk = null;

        if (x < 0)
        {
            chunk = m_CachedChunks[LEFT];
            x = 16 + x;
        }
        else if (x > 15)
        {
            chunk = m_CachedChunks[RIGHT];
            x = 16 - x;
        }
        else if (z < 0)
        {
            chunk = m_CachedChunks[FRONT];
            z = 16 + z;
        }
        else if (z > 15)
        {
            chunk = m_CachedChunks[BACK];
            z = 16 - z;
        }
        else
        {
            chunk = m_CachedChunks[CENTER];
        }

        return chunk.getBlockId(x, m_ChunkY * ChunkRenderer.CHUNK_SIZE + y, z);
    }
}