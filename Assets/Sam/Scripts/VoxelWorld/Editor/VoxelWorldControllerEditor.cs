using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

class AutoRectHelper {

    private Rect startRect;

    public AutoRectHelper(Rect rect) {
        this.startRect = new Rect(rect.x, rect.y, rect.width, rect.height);
    }

    public Rect getNext() {
        return new Rect(this.startRect.x, this.startRect.y += EditorGUIUtility.singleLineHeight, this.startRect.width, EditorGUIUtility.singleLineHeight);
    }

}

[CustomEditor(typeof(VoxelWorldController))]
[CanEditMultipleObjects]
public class VoxelWorldControllerEditor : Editor {

    public VoxelWorldController voxelController;
    private ReorderableList list;
    private Vector2 scrollOffset = new Vector2();

    private void OnEnable() {
        this.voxelController = (VoxelWorldController)target;

        if(this.voxelController.blocks == null) {
            this.voxelController.blocks = new List<Block>();
        }

        this.list = new ReorderableList(this.voxelController.blocks, typeof(Block), true, true, true, true);
        this.list.drawElementCallback = onBlockItemRender;
        this.list.elementHeightCallback = onBlockItemGetHeight;
        this.list.onReorderCallback = (ReorderableList list) => {
            setDirty();
        };
        this.list.onChangedCallback = (ReorderableList list) => {
            setDirty();
        };
        this.list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Block defines");
        };
    }

    private float onBlockItemGetHeight(int index) {
        Block block = this.voxelController.blocks[index];
        return block.isOpen ? (200 + EditorGUIUtility.singleLineHeight * 4) : 20;
    }

    private void onBlockItemRender(Rect rect, int index, bool isActive, bool isFocused) {
        //SerializedProperty property = list.serializedProperty.GetArrayElementAtIndex(index);
        Block block = this.voxelController.blocks[index];
        block.id = index + 1;
        rect.y += 2;

        AutoRectHelper rh = new AutoRectHelper(rect);

        block.isOpen = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), block.isOpen, "Block ID#" + block.id + (block.name.Length > 0 ? (" - " + block.name) : ""));
        if (!block.isOpen) return;

        //EditorGUI.LabelField(rect, "Block ID#" + block.id);
        block.name = EditorGUI.TextField(rh.getNext(), "Name", block.name);
        block.meshType = (VoxelMeshType)EditorGUI.EnumPopup(rh.getNext(), "Mesh type", block.meshType);
        block.isTransparent = EditorGUI.ToggleLeft(rh.getNext(), "Is transparent", block.isTransparent);
        GUI.enabled = block.isTransparent;
        block.renderAllFaces = EditorGUI.ToggleLeft(rh.getNext(), "Render inside", block.renderAllFaces);
        GUI.enabled = true;
        block.isSolid = EditorGUI.ToggleLeft(rh.getNext(), "Is solid", block.isSolid);
        block.isDestroyable = EditorGUI.ToggleLeft(rh.getNext(), "Is destroyable", block.isDestroyable);
        GUI.enabled = block.isDestroyable;
        block.hardness = EditorGUI.IntField(rh.getNext(), "Hardness", block.hardness);
        GUI.enabled = true;
        block.hardness = Mathf.Clamp(block.hardness, 0, 9999);
        block.height = EditorGUI.Slider(rh.getNext(), "Height", block.height, 0.0f, 1.0f);
        rh.getNext();
        EditorGUI.LabelField(rh.getNext(), "Block textures");
        bool texOldState = block.faceTexturePos == null || (block.faceTexturePos != null && block.faceTexturePos.Length == 1);
        GUI.enabled = block.meshType == VoxelMeshType.Block;
        bool texSame = EditorGUI.ToggleLeft(rh.getNext(), "Same faces", texOldState);
        if(block.meshType == VoxelMeshType.Cross) {
            texSame = true;
        }
        GUI.enabled = true;
        
        if (texOldState != texSame || block.faceTexturePos == null || block.editorTexturesPreview == null) {
            if(texSame) {
                block.faceTexturePos = new Vector2[1];
                block.editorTexturesPreview = new Texture[1];
            } else {
                block.faceTexturePos = new Vector2[6];
                block.editorTexturesPreview = new Texture[6];
            }
        }
        
        Rect texRect = rh.getNext();
        for (int i = 0; i < (texSame ? 1 : 6); i++) {
            if(GUI.Button(new Rect(texRect.x + i * 55, texRect.y, 50, texRect.height), texSame ? "Select" : Enum.GetName(typeof(BlockFace), (BlockFace)i))) {
                Material mat = block.isTransparent ? this.voxelController.transparentMaterial : this.voxelController.opaqueMaterial;
                if(mat != null) {
                    VoxelWorldBlockTextureSelectorWindow selectionWindow = EditorWindow.GetWindow<VoxelWorldBlockTextureSelectorWindow>(true);
                    selectionWindow.setData(this, block, i, this.list, mat);
                } else {
                    EditorUtility.DisplayDialog("VoxelWorld cannot proceed", "Please select both materials in editor !", "Ok");
                }
            }
            Rect p = new Rect(texRect.x + i * 55, texRect.y + 2 + EditorGUIUtility.singleLineHeight, 50, 50);
            if(block.editorTexturesPreview[i] != null) {
                EditorGUI.DrawTextureTransparent(p, block.editorTexturesPreview[i]);
            } else {
                EditorGUI.DrawRect(p, Color.gray);
            }
        }

        EditorGUI.DrawRect(new Rect(texRect.x, texRect.y + 70, texRect.width, 3), Color.gray);



        //EditorGUI.IntField(new Rect(rect.x, rect.y + 25, 60, EditorGUIUtility.singleLineHeight), new GUIContent("asd"), 0);

        //setDirty();

        if(GUI.changed) {
            setDirty();
        }
    }

    public void setDirty() {
        if(!EditorApplication.isPlaying) {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        this.scrollOffset = EditorGUILayout.BeginScrollView(this.scrollOffset);
        base.OnInspectorGUI();

        if (GUILayout.Button("Restart save")) {
            this.voxelController.worldSaver.deleteSave();
        }

        GUILayout.Space(5);

        if (this.voxelController.blocks.Count < 6) {
            EditorGUILayout.HelpBox("You must have at least 4 blocks !", MessageType.Warning);
        }

        EditorGUILayout.HelpBox("1 = Stone, 2 = Dirt, 3 = Grass, 4 = Bedrock, 5 = Wood, 6 = Leaves", MessageType.Info);

        this.list.DoLayoutList();

        //GUI.Window(1,GUILayoutUtility.GetRect(100, 200), onWindow, new GUIContent("Ya"));

        EditorGUILayout.EndScrollView();
        serializedObject.ApplyModifiedProperties();
    }

    public void onWindow(int id) {

    }

}
