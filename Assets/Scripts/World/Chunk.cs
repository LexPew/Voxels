using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class: Chunk class repsents a cubic chunk in the game world.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    //Chunk Mesh Data
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    int currentIndex = 0;


    //Chunk Data
    [SerializeField] private GameObject voxelPrefab;
    //Change to static later
    [SerializeField] private int cubicSize = 16;

    public bool generateOnStart = true;

    //Block Array That holds 0-1 for block or air
    [SerializeField] private int[] chunkVoxels;
    void Start()
    {

        GrabMeshComponents();
        PopulateChunk();

        if (generateOnStart)
        {
            GenerateChunk();
        }
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    private void GrabMeshComponents()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        mesh = new Mesh();
    }
    void PopulateChunk()
    {
        //Set Array Value
        chunkVoxels = new int[cubicSize * cubicSize * cubicSize];

        //Fill with full values
        for (int x = 0; x < cubicSize; x++)
        {
            for (int y = 0; y < cubicSize; y++)
            {
                for (int z = 0; z < cubicSize; z++)
                {

                    SetVoxel(x, y, z, (x + y + z )% 2);
                }
            }
        }
    }

    [ContextMenu("Generate Chunk")]
    void GenerateChunk()
    {
        //Now generate faces based on whether they are needed, so traverse through each block and check its neighbours in the voxel array 
        for (int x = 0; x < cubicSize; x++)
        {
            for (int y = 0; y < cubicSize; y++)
            {
                for (int z = 0; z < cubicSize; z++)
                {
                    //Check if it is airblock or not then gen faces
                    if(GetVoxel(x,y,z) == 0) continue;
                    //Append Faces
                    AddFaces(x, y, z);

                }
            }
        }
    }


    //Helper functions, 3D position to 1D array
    int GetVoxel(int x, int y, int z)
    {
        //Check its a valid position
        if (x < 0 || x >= cubicSize || y < 0 || y >= cubicSize || z < 0 || z >= cubicSize)
        {
            return -1;
        }

        //Read from the chunkVoxels array
        return chunkVoxels[x + y * cubicSize + z * cubicSize * cubicSize];
    }
    void SetVoxel(int x, int y, int z, int value)
    {
        //Set value in chunkVoxels array
        chunkVoxels[x + cubicSize * (y + cubicSize * z)] = value;
    }

    void AddFaces(int x, int y, int z)
    {

        Vector3 basePos = new Vector3(x, y, z);
        //Check each neighbour
        for(int f = 0; f < 6; f++)
        {
            Vector3Int dir = Block.faceDirs[f]; 

            int newX = x + dir.x;
            int newY = y + dir.y;
            int newZ = z + dir.z;

            //Will return -1,0 or 1
            int neighbour = GetVoxel(newX,newY,newZ);

            //Draw if air our or border
            bool draw =  (neighbour == 0) || (neighbour == -1);

            if (draw)
            {
                //Cast f to block face
                AppendFace(basePos, (Block.Face)f);
            }
        }
    }

    void AppendFace(Vector3 position, Block.Face face)
    {
          for (int i = 0; i < 4; i++)
            {
                vertices.Add(Block.faceVertices[(int)face, i] + position);

            }
            for (int i = 0; i < 6; i++)
            {
                triangles.Add(currentIndex + Block.voxelTris[i]);
            }
            currentIndex += 4;
    }
}
