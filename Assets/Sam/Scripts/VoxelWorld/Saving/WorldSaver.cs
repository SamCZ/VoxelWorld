using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldSaver {

    private string folder;
    private bool isFolderExist;
    private IDictionary<Vector2Int, Dictionary<Vector3Int, byte>> chunkBlocks;

    public WorldSaver(string folder) {
        this.folder = folder;
        this.isFolderExist = Directory.Exists(this.folder);
        this.chunkBlocks = new Dictionary<Vector2Int, Dictionary<Vector3Int, byte>>();
    }

    public Dictionary<Vector3Int, byte> getSavedBlocks(int x, int z) {
        if (!this.isFolderExist) return null;
        Vector2Int loc = new Vector2Int(x, z);
        string filename = "chunk_" + x + "_" + z + ".dat";
        string filePath = Path.Combine(this.folder, filename);
        if(File.Exists(filePath)) {
            if(this.chunkBlocks.ContainsKey(loc)) {
                return this.chunkBlocks[loc];
            } else {
                Dictionary<Vector3Int, byte> storage = new Dictionary<Vector3Int, byte>();
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                    using (BinaryReader br = new BinaryReader(fs)) {
                        int blockCount = br.ReadInt32();
                        for (int i = 0; i < blockCount; i++) {
                            storage.Add(new Vector3Int(br.ReadInt32(), br.ReadInt32(), br.ReadInt32()), br.ReadByte());
                        }
                    }
                }
                this.chunkBlocks.Add(loc, storage);
                return storage;
            }
        }
        return null;
    }

    public void deleteSave() {
        foreach(string file in Directory.GetFiles(this.folder)) {
            File.Delete(file);
        }
        Directory.Delete(this.folder);
        this.isFolderExist = false;
    }

    // Ukládání a načítání rotatce nefunguje asi díky tomu default chracteru.
    public bool loadPlayerLocation(Transform transform) {
        string filename = "playerdata.dat";
        string filePath = Path.Combine(this.folder, filename);
        if (File.Exists(filePath)) {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                using (BinaryReader br = new BinaryReader(fs)) {
                    transform.parent.position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    transform.eulerAngles = new Vector3(br.ReadSingle(), 0, 0);
                    transform.parent.eulerAngles = new Vector3(0, br.ReadSingle(), 0);
                    return true;
                }
            }
        }
        return false;
    }

    public void savePlayerLocation(Transform transform) {
        string filename = "playerdata.dat";
        string filePath = Path.Combine(this.folder, filename);
        if (!Directory.Exists(this.folder)) {
            Directory.CreateDirectory(this.folder);
        }
        using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)) {
            using (BinaryWriter bw = new BinaryWriter(fs)) {
                bw.Write(transform.parent.position.x);
                bw.Write(transform.parent.position.y);
                bw.Write(transform.parent.position.z);
                bw.Write(transform.eulerAngles.x);
                bw.Write(transform.parent.eulerAngles.y);
            }
        }
    }

    public void saveAll() {
        foreach(KeyValuePair<Vector2Int, Dictionary<Vector3Int, byte>> dc in this.chunkBlocks) {
            Vector2Int cl = dc.Key;
            string filename = "chunk_" + cl.x + "_" + cl.y + ".dat";
            string filePath = Path.Combine(this.folder, filename);
            using(FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)) {
                using (BinaryWriter bw = new BinaryWriter(fs)) {
                    bw.Write(dc.Value.Count);
                    foreach(KeyValuePair<Vector3Int, byte> bd in dc.Value) {
                        bw.Write(bd.Key.x);
                        bw.Write(bd.Key.y);
                        bw.Write(bd.Key.z);
                        bw.Write(bd.Value);
                    }
                }
            }
        }
    }

    public void onBlockUpdate(int x, int y, int z, byte blockId) {
        Vector2Int cloc = new Vector2Int(x >> 4, z >> 4);
        Vector3Int bloc = new Vector3Int(x & 15, y, z & 15);
        Dictionary<Vector3Int, byte> bdic;
        if (this.chunkBlocks.ContainsKey(cloc)) {
            bdic = this.chunkBlocks[cloc];
        } else {
            bdic = new Dictionary<Vector3Int, byte>();
            this.chunkBlocks[cloc] = bdic;
        }

        if(bdic.ContainsKey(bloc)) {
            bdic[bloc] = blockId;
        } else {
            bdic.Add(bloc, blockId);
        }
    }
}
