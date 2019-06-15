using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkProviderGenratorFlat : IChunkProvider {

    public Chunk getChunk(int x, int z) {
        Chunk chunk = new Chunk(x, z);
        for (int bx = 0; bx < 16; bx++) {
            for (int bz = 0; bz < 16; bz++) {
                for (int y = 0; y < 5; y++) {
                    byte blockId = 4;
                    if(y == 4) {
                        blockId = 3;
                    }  else if(y > 0) {
                        blockId = 2;
                    }
                    chunk.setBlock(bx, y, bz, blockId);
                }
            }
        }

        return chunk;
    }

}
