using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelInterface : MonoBehaviour {

    public Texture crosshairTexture;
    public float crosshairScale = 1.0f;
    public VoxelWorldController voxelWorld;
    public Material wireMaterial;
    public Material breakingMaterial;
    public Texture[] breakingSprites;
    public Texture barTexture;
    public Texture barTileTexture;
    public Font itemTextFont;

    private GameObject voxelPickerObj;
    private Vector3 pickBlockOffset = new Vector3(0.5f, 0.5f, 0.5f);
    private GameObject breakingBlock;

    private int selectedBlock;
    private bool isBreaking;
    private float breakClickTime;

    void Start () {
        this.breakingBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        this.breakingBlock.GetComponent<BoxCollider>().enabled = false;
        this.breakingBlock.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
        this.breakingBlock.GetComponent<MeshRenderer>().material = this.breakingMaterial;
        this.breakingBlock.SetActive(false);
    }

    private void setBreakingTexture(float f) {
        this.breakingMaterial.SetTexture("_MainTex", this.breakingSprites[Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(f) * this.breakingSprites.Length), 0, this.breakingSprites.Length-1)]);
    }
 	
	void Update () {
        if(this.voxelPickerObj == null) {
            this.voxelPickerObj = MeshBuilder.createWireframePicker(this.voxelWorld.gameObject, this.wireMaterial);
        }

        Vector3 lookingAt;
        Vector3 placeAt;
        byte blockId;
        if (this.voxelWorld.getPlayerBlockPick(this.transform, out lookingAt, out placeAt, out blockId)) {
            this.voxelPickerObj.SetActive(true);
            this.voxelPickerObj.transform.position = lookingAt + this.pickBlockOffset;

            this.breakingBlock.transform.position = lookingAt + this.pickBlockOffset;

            if (Input.GetMouseButtonDown(0)) {
                this.isBreaking = true;
                this.breakingBlock.SetActive(true);
                this.breakClickTime = Time.time;
            } else if(Input.GetMouseButtonUp(0)) {
                this.isBreaking = false;
                this.breakingBlock.SetActive(false);
            }

            if (Input.GetMouseButtonDown(1)) {
                Block block = this.voxelWorld.getBlocks()[this.selectedBlock];
                if (block.isDestroyable) this.voxelWorld.getWorld().setBlockId(placeAt, (byte)block.id);
            }

            if (this.isBreaking) {
                Block block = this.voxelWorld.getBlockInstance(blockId);
                float timeElapsed = Time.time - this.breakClickTime;
                float breakTime = block.hardness * 0.5f;
                if(block.isDestroyable) {
                    setBreakingTexture(timeElapsed / breakTime);
                    if (timeElapsed >= breakTime) {
                        this.voxelWorld.getWorld().setBlockId(lookingAt, 0);
                        this.breakClickTime = Time.time;
                    }
                } else {
                    setBreakingTexture(0);
                }
            }
        } else {
            this.isBreaking = false;
            this.breakingBlock.SetActive(false);
            this.voxelPickerObj.SetActive(false);
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0) {
            this.selectedBlock--;
        } else if (Input.GetAxis("Mouse ScrollWheel") < 0) {
            this.selectedBlock++;
        }
    }

    void OnGUI() {
        if (this.voxelWorld == null) return;

        List<Block> blocks = this.voxelWorld.getBlocks();
        this.selectedBlock = Mathf.Clamp(this.selectedBlock, 0, blocks.Count-1);

        int padding = 8;
        int ts = this.barTileTexture.width - padding * 2;
        int sw = this.barTexture.width;
        int sh = this.barTexture.height;

        int sx = Screen.width / 2 - sw / 2;
        int sy = Screen.height - sh;

        GUI.DrawTexture(new Rect(sx, sy, sw, sh), this.barTexture);

        for (int i = 0; i < blocks.Count; i++) {
            if(this.selectedBlock == i) {
                GUI.DrawTexture(new Rect(sx + i * (this.barTileTexture.width - padding), sy, this.barTileTexture.width, this.barTileTexture.height), this.barTileTexture);
            }
            GUI.DrawTexture(new Rect(sx + i * (this.barTileTexture.width - padding) + padding, sy + this.barTileTexture.width / 2 - ts / 2, ts, ts), blocks[i].editorTexturesPreview[0]);
        }

        GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        GUI.skin.font = this.itemTextFont;
        GUI.Label(new Rect(0, Screen.height - sh - 40, Screen.width, 35), blocks[this.selectedBlock].name, centeredStyle);


        if (this.crosshairTexture != null) {
            if (this.crosshairTexture != null) {
                float w = this.crosshairTexture.width * this.crosshairScale;
                float h = this.crosshairTexture.height * this.crosshairScale;
                GUI.DrawTexture(new Rect(Screen.width / 2 - w / 2, Screen.height / 2 - h / 2, w, h), this.crosshairTexture);
            }
        }
    }
}
