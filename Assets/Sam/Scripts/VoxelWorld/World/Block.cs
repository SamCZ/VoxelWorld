using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockFace {
    Top = 0,
    Bottom = 1,
    Left = 2,
    Right = 3,
    Front = 4,
    Back = 5
}

[System.Serializable]
public class Block {

    public static Block AIR = new Block();

    public static byte AIR_ID = 0;
    public static byte STONE_ID = 1;
    public static byte DIRT_ID = 2;
    public static byte GRASS_ID = 3;
    public static byte BEDROCK_ID = 4;
    public static byte WOOD_ID = 5;
    public static byte LEAVES_ID = 6;

    [SerializeField]
    public int id;
    [SerializeField]
    public string name = "Block";
    [SerializeField]
    public bool isTransparent;
    [SerializeField]
    public bool renderAllFaces = false;
    [SerializeField]
    public bool isSolid = true;
    [SerializeField]
    public bool isDestroyable = true;
    [SerializeField]
    public int hardness = 1;
    [SerializeField]
    public Vector2[] faceTexturePos;
    [SerializeField]
    public VoxelMeshType meshType = VoxelMeshType.Block;
    [SerializeField]
    public float height = 1.0f;
    
    [SerializeField]
    public Texture[] blockTexturesPreview;
    [SerializeField]
    public bool isOpen = true;
}
