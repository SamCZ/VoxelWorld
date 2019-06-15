using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder {

    public static MeshBuilder OPAQUE_BUILDER = new MeshBuilder();
    public static MeshBuilder TRANSPARENT_BUILDER = new MeshBuilder();

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uv = new List<Vector2>();
    public List<Color> colors = new List<Color>();
    public List<Vector3> colVertices = new List<Vector3>();
    public List<int> colTriangles = new List<int>();

    private int squareCount;
    private int colCount;

    public MeshBuilder() {

    }

    public void addQuad(float[] par1, float[] par2, float[] par3, float[] par4, bool genCollider) {
        vertices.Add(new Vector3(par1[0], par1[1], par1[2]));
        vertices.Add(new Vector3(par2[0], par2[1], par2[2]));
        vertices.Add(new Vector3(par3[0], par3[1], par3[2]));
        vertices.Add(new Vector3(par4[0], par4[1], par4[2]));

        triangles.Add(squareCount * 4);
        triangles.Add((squareCount * 4) + 1);
        triangles.Add((squareCount * 4) + 3);
        triangles.Add((squareCount * 4) + 1);
        triangles.Add((squareCount * 4) + 2);
        triangles.Add((squareCount * 4) + 3);
        squareCount++;

        if (genCollider) {
            this.addQuadCollider(par1, par2, par3, par4);
        }
    }

    public void addQuad(float[] par1, float[] par2, float[] par3, float[] par4) {
        this.addQuad(par1, par2, par3, par4, false);
    }

    public void addQuadCollider(float[] par1, float[] par2, float[] par3, float[] par4) {
        colVertices.Add(new Vector3(par1[0], par1[1], par1[2]));
        colVertices.Add(new Vector3(par2[0], par2[1], par2[2]));
        colVertices.Add(new Vector3(par3[0], par3[1], par3[2]));
        colVertices.Add(new Vector3(par4[0], par4[1], par4[2]));

        colTriangles.Add(colCount * 4);
        colTriangles.Add((colCount * 4) + 1);
        colTriangles.Add((colCount * 4) + 3);
        colTriangles.Add((colCount * 4) + 1);
        colTriangles.Add((colCount * 4) + 2);
        colTriangles.Add((colCount * 4) + 3);
        colCount++;
    }

    public void addQuadCollider(Vector3 par1, Vector3 par2, Vector3 par3, Vector3 par4) {
        this.addQuadCollider(
               new float[] { par1.x, par1.y, par1.z
            }, new float[] { par2.x, par2.y, par2.z
            }, new float[] { par3.x, par3.y, par3.z
            }, new float[] { par4.x, par4.y, par4.z
        });
    }

    public void addUV(float u, float v) {
        this.uv.Add(new Vector2(u, v));
    }

    public void addColor(float r, float g, float b, float a) {
        this.colors.Add(new Color(r, g, b, a));
    }

    public void addColor(float r, float g, float b) {
        this.colors.Add(new Color(r, g, b, 1F));
    }

    public bool isEmpty() {
        return this.vertices.Count == 0;
    }

    public void clear() {
        this.vertices.Clear();
        this.triangles.Clear();
        this.uv.Clear();
        this.colors.Clear();
        this.colVertices.Clear();
        this.colTriangles.Clear();
        this.squareCount = 0;
        this.colCount = 0;
    }

    public void UploadData(GameObject obj, Material material) {
        if (obj == null) return;

        if(obj.GetComponent<MeshRenderer>() == null) {
            obj.AddComponent<MeshRenderer>();
        }

        MeshFilter filter = obj.GetComponent<MeshFilter>();
        if(filter == null) {
            filter = obj.AddComponent<MeshFilter>();
        }

        MeshCollider collider = obj.GetComponent<MeshCollider>();
        if(collider == null) {
            collider = obj.AddComponent<MeshCollider>();
        }

        Renderer renderer = obj.GetComponent<Renderer>();
        if(renderer == null) {
            renderer = obj.AddComponent<Renderer>();
        }
        renderer.material = material;

        Mesh mesh = filter.mesh;
        mesh.MarkDynamic();

        mesh.Clear();
        mesh.vertices = this.vertices.ToArray();
        mesh.triangles = this.triangles.ToArray();
        mesh.uv = this.uv.ToArray();
        mesh.colors = this.colors.ToArray();
        mesh.RecalculateNormals();

        Mesh newMesh = new Mesh();
        newMesh.vertices = this.colVertices.ToArray();
        newMesh.triangles = this.colTriangles.ToArray();
        collider.sharedMesh = newMesh;
    }

    public static GameObject createWireframePicker(GameObject parent, Material material) {
        GameObject obj = new GameObject("Picker");
        obj.transform.parent = parent.transform;
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        MeshFilter filter = obj.AddComponent<MeshFilter>();

        renderer.material = material;

        Mesh mesh = filter.mesh;

        float size = 1.0f + 0.01f;

        mesh.vertices = new Vector3[] {
            new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f), //0
            new Vector3(-size / 2.0f, -size / 2.0f, size / 2.0f),  //1
            new Vector3(size / 2.0f, -size / 2.0f, size / 2.0f),   //2
            new Vector3(size / 2.0f, -size / 2.0f, -size / 2.0f),  //3

            new Vector3(-size / 2.0f, size / 2.0f, -size / 2.0f),  //4
            new Vector3(-size / 2.0f, size / 2.0f, size / 2.0f),   //5
            new Vector3(size / 2.0f, size / 2.0f, size / 2.0f),    //6
            new Vector3(size / 2.0f, size / 2.0f, -size / 2.0f),   //7
        };

        mesh.SetIndices(new int[] {
            0, 1,
            1, 2,
            2, 3,
            3, 0,

            4, 5,
            5, 6,
            6, 7,
            7, 4,

            0, 4, 
            1, 5,
            2, 6,
            3, 7
        }, MeshTopology.Lines, 0);

        return obj;
    }

}
