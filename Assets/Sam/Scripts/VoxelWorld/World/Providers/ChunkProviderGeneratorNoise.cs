using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkProviderGeneratorNoise : IChunkProvider {

    private int seed;
    private FastNoise noise;
    private float[,] noiceCache;
    private List<Block> vegentationBlocks = new List<Block>();

    public ChunkProviderGeneratorNoise(int seed, List<Block> blocks) {
        this.seed = seed;

        this.noise = new FastNoise();
        this.noise.SetNoiseType(FastNoise.NoiseType.Simplex);

        this.noiceCache = new float[16 + 2, 16 + 2];

        foreach(Block b in blocks) {
            if(b.meshType == VoxelMeshType.Cross) {
                this.vegentationBlocks.Add(b);
            }
        }
    }

    private float getValueFromCache(int x, int y) {
        if (x < 0 || x >= 18 || y < 0 || y >= 18) return 0;
        return this.noiceCache[x, y];
    }

    public Chunk getChunk(int x, int z) {
        Chunk chunk = new Chunk(x, z);

        Random.InitState(0);

        for (int bx = 0; bx < 16 + 2; bx++) {
            for (int bz = 0; bz < 16 + 2; bz++) {
                float fx = (x * ChunkRenderer.CHUNK_SIZE + bx);
                float fz = (z * ChunkRenderer.CHUNK_SIZE + bz);

                float height = scaled_octave_noise_2d(6, 0.45f, 0.5f, 0, 10, fx, fz) * 1.8f;

                float biome = this.noise.GetNoise(fx * 0.8f, fz * 0.8f);
                if (biome > 0.4) {
                    height += scaled_octave_noise_2d(6, 1.85f, 0.25f, 0, 10, fx, fz);
                }

                this.noiceCache[bx, bz] = Mathf.Clamp(height, 0, 16);
            }
        }

        for (int bx = 1; bx < 16 + 2; bx++) {
            for (int bz = 1; bz < 16 + 2; bz++) {

                float left = getValueFromCache(bx - 1, bz);
                float right = getValueFromCache(bx + 1, bz);
                float top = getValueFromCache(bx, bz + 1);
                float bottom = getValueFromCache(bx, bz - 1);

                this.noiceCache[bx, bz] = (left + right + top + bottom) / 4.0f;
            }
        }

        bool haveTree = false;

        for (int bx = 0; bx < 16; bx++) {
            for (int bz = 0; bz < 16; bz++) {

                float fx = (x * ChunkRenderer.CHUNK_SIZE + bx);
                float fz = (z * ChunkRenderer.CHUNK_SIZE + bz);
                
                int height = Mathf.Clamp(Mathf.FloorToInt(this.noiceCache[bx+1, bz+1]), 0, 16);

                int dirtLevel = 3;

                int y;
                for (y = 0; y < height; y++) {
                    chunk.setBlock(bx, y, bz, Block.STONE_ID);
                    if(y > height - 1 - dirtLevel) {
                        chunk.setBlock(bx, y, bz, Block.DIRT_ID);
                    }
                    if(y == height-1) {
                        chunk.setBlock(bx, y, bz, Block.GRASS_ID);
                    }
                }

                bool tree = false;
                if(bx > 2 && bx < 15 - 2 && bz > 2 && bz < 15 - 2 && y < 15 - 6 && Random.value > 0.99f && !haveTree) {
                    this.plantTree(chunk, bx, y, bz);
                    haveTree = true;
                    tree = true;
                }

                if(Random.value > 0.9f && !tree && this.vegentationBlocks.Count > 0) {
                    chunk.setBlock(bx, y, bz, (byte)this.vegentationBlocks[Random.Range(0, this.vegentationBlocks.Count)].id);
                }

                chunk.setBlock(bx, 0, bz, Block.BEDROCK_ID);
            }
        }

        return chunk;
    }

    private void plantTree(Chunk chunk, int x, int y, int z) {
        int height = Random.Range(4, 6);
        int leavesSize = 2;

        for (int i = 0; i < height; i++) {
            chunk.setBlock(x, y + i, z, Block.WOOD_ID);
        }

        for (int ly = height / 2; ly < height + 2; ly++) {

            if (ly > height) leavesSize--;

            for (int lx = -leavesSize; lx <= leavesSize; lx++) {
                for (int lz = -leavesSize; lz <= leavesSize; lz++) {

                    if (ly < height && lx == 0 && lz == 0) continue;
                    if (chunk.getBlockId(x + lx, y + ly, z + lz) == 0) {
                        chunk.setBlock(x + lx, y + ly, z + lz, Block.LEAVES_ID);
                    }

                }
            }
        }

        chunk.setBlock(x, y + height + 2, z, Block.LEAVES_ID);
    }

    private float scaled_octave_noise_2d(float octaves, float persistence, float scale, float loBound, float hiBound, float x, float y) {
        return octave_noise_2d(octaves, persistence, scale, x, y) * (hiBound - loBound) / 2 + (hiBound + loBound) / 2;
    }

    private float octave_noise_2d(float octaves, float persistence, float scale, float x, float y) {
        float total = 0;
        float frequency = scale;
        float amplitude = 1;
        
        float maxAmplitude = 0;
        
        for (int i = 0; i < octaves; i++) {
            total += this.noise.GetNoise(x * frequency, y * frequency) * amplitude;

            frequency *= 2;
            maxAmplitude += amplitude;
            amplitude *= persistence;
        }

        return total / maxAmplitude;
    }

}
