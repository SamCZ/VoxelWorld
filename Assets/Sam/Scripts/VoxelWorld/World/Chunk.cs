using System.Collections;
using System.Collections.Generic;

public class Chunk
{
    public static int CHUNK_HEIGHT = 16 * 16;

    private int x;
    private int y;
    private byte[] chunkArray;
    private byte[] heightMap;

    public Chunk(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.chunkArray = new byte[16 * CHUNK_HEIGHT * 16];
        this.heightMap = new byte[16 * 16];
    }

    public void updateHeightMap()
    {
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                for (int y = CHUNK_HEIGHT - 1; y >= 0; y--)
                {
                    if (getBlockId(x, y, z) > 0)
                    {
                        this.heightMap[x + z * 16] = (byte)y;
                        break;
                    }
                }
            }
        }
    }

    public int getMaxHeight(int x, int z)
    {
        if (isOutside(x, 0, z)) return 0;
        return this.heightMap[x + z * 16];
    }

    public void setBlock(int x, int y, int z, byte blockId)
    {
        if (isOutside(x, y, z)) return;
        this.chunkArray[getIndex(x, y, z)] = blockId;
    }

    public byte getBlockId(int x, int y, int z)
    {
        if (isOutside(x, y, z)) return 0;
        return this.chunkArray[getIndex(x, y, z)];
    }

    public static int getIndex(int x, int y, int z)
    {
        return (x * 16 + z) * CHUNK_HEIGHT + y;
    }

    private static bool isOutside(int x, int y, int z)
    {
        return x < 0 || x > 15 || y < 0 || y > (CHUNK_HEIGHT - 1) || z < 0 || z > 15;
    }

}
