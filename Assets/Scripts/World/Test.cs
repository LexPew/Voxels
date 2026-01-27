using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Test : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private int currentIndex = 0;
    public void Start()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        mesh = new Mesh();



        //Loop through each face
        for (int f = 0; f < 6; f++)
        {
            //Loop through the verts and add them
            for (int v = 0; v < 4; v++)
            {
                vertices.Add(Block.faceVertices[f, v]);
            }
            //Loop through the indices and add them with the index offset
            for(int i = 0; i < 6; i++)
            {
                triangles.Add(currentIndex +Block.voxelTris[i]);
            }
            currentIndex+= 4;
        }





        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

}