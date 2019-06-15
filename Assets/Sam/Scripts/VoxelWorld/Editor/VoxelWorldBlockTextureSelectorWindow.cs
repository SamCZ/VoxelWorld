using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class VoxelWorldBlockTextureSelectorWindow : EditorWindow {

    private VoxelWorldControllerEditor editor;
    private ReorderableList list;
    private Material selectedMaterial;
    private Texture blocksTexture;
    private Block selectedBlock;
    private int selectedFace;

    private void OnEnable() {
        this.titleContent.text = "VoxelWorld Texture Selection";
    }

	public void OnGUI() {
        Event e = Event.current;

        int texSize = this.editor.voxelController.textureSize;
        int tileSize = this.editor.voxelController.tileSize;

        int resized = 512;

        float tileScale = (float)resized / (float)texSize;
        int rescaledTileSize = Mathf.FloorToInt(tileScale * tileSize);

        Rect rect = new Rect(0, 0, resized, resized);
        EditorGUI.DrawTextureTransparent(rect, this.blocksTexture);

        int mlx = Mathf.FloorToInt(e.mousePosition.x / rescaledTileSize);
        int mly = Mathf.FloorToInt(e.mousePosition.y / rescaledTileSize);

        int cx = mlx * rescaledTileSize;
        int cy = mly * rescaledTileSize;

        if(cx < this.blocksTexture.width && cy < this.blocksTexture.height) {
            EditorGUI.DrawRect(new Rect(cx, cy, rescaledTileSize, rescaledTileSize), Color.green);

            if(e.isMouse && e.button == 0 && e.type == EventType.MouseDown) {
                Texture2D tex = (Texture2D)this.blocksTexture;

                int rpx = Mathf.FloorToInt(cx / tileScale);
                int rpy = Mathf.FloorToInt(cy / tileScale);
                
                int py = tex.height - rpy - tileSize;
                if (rpx < 0 || rpx >= tex.width || rpy < 0 || rpy >= tex.width) return;
                
                Color[] blockPixels = tex.GetPixels(rpx, py, tileSize, tileSize, 0);

                Texture2D newTex = new Texture2D(tileSize, tileSize);
                newTex.SetPixels(blockPixels);
                newTex.filterMode = FilterMode.Point;
                newTex.hideFlags = HideFlags.DontSave;
                newTex.Apply();

                this.selectedBlock.blockTexturesPreview[this.selectedFace] = newTex;
                this.selectedBlock.faceTexturePos[this.selectedFace] = new Vector2(mlx, mly);

                this.editor.setDirty();

                this.Close();
            }
        }

        this.Repaint();
    }

    public void setData(VoxelWorldControllerEditor editor, Block block, int face, ReorderableList list, Material material) {
        this.editor = editor;
        this.selectedFace = face;
        this.selectedBlock = block;
        this.list = list;
        this.selectedMaterial = material;

        this.blocksTexture = this.selectedMaterial.GetTexture("_MainTex");
    }

}
