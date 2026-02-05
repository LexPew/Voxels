using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

//Class: Chunk class repsents a cubic chunk in the game world.

public class Chunk
{
    World world;
    public GameObject gameObject;


    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;


    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector3> uvs = new List<Vector3>();

    int currentIndex = 0;

    [SerializeField] private int[] chunkData;

    ChunkPosition chunkPosition;


    //Contstructor
    public Chunk(ChunkPosition _chunkPosition, World _world)
    {
        world = _world;

        gameObject = new();
        gameObject.transform.parent = world.transform;

        chunkPosition = _chunkPosition;
        Vector3 worldPos = new Vector3(chunkPosition.x, chunkPosition.y, chunkPosition.z) * WorldData.chunkSize;
        gameObject.transform.position = worldPos;
        gameObject.name = $"Chunk: {chunkPosition.x}, {chunkPosition.y}, {chunkPosition.z}";

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.masterMaterial;

        mesh = new();

        PopulateChunk();
    }
    //Used once the chunk is created to generate and draw the chunk, delayed to allow for chunk population first and its neighbours
    public void DrawChunk()
    {
        GenerateChunk();
        SetupMesh();
    }
    void SetupMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, uvs.ToArray());
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    void PopulateChunk()
    {
        //Set Array Value
        chunkData = new int[WorldData.chunkSize * WorldData.chunkSize * WorldData.chunkSize];

        //Fill with full values
        for (int x = 0; x < WorldData.chunkSize; x++)
        {
            for (int y = 0; y < WorldData.chunkSize; y++)
            {
                for (int z = 0; z < WorldData.chunkSize; z++)
                {
                    SetVoxel(x, y, z, world.GetVoxel(new Vector3(x, y, z), chunkPosition));
                }
            }
        }
    }

    void GenerateChunk()
    {
        //Now generate faces based on whether they are needed, so traverse through each block and check its neighbours in the voxel array 
        for (int x = 0; x < WorldData.chunkSize; x++)
        {
            for (int y = 0; y < WorldData.chunkSize; y++)
            {
                for (int z = 0; z < WorldData.chunkSize; z++)
                {
                    //Check if it is airblock or not then gen faces
                    if (GetVoxel(x, y, z) == 0) continue;
                    //Append Faces
                    AddFaces(x, y, z);

                }
            }
        }
    }


    //Helper functions, 3D position to 1D array
    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x >= WorldData.chunkSize || y < 0 || y >= WorldData.chunkSize || z < 0 || z >= WorldData.chunkSize)
        {
            return false;
        }
        return true;
    }
    int GetVoxel(int x, int y, int z)
    {
        //Read from the chunkVoxels array
        return chunkData[x + y * WorldData.chunkSize + z * WorldData.chunkSize * WorldData.chunkSize];
    }
    void SetVoxel(int x, int y, int z, int value)
    {
        //Set value in chunkVoxels array
        chunkData[x + WorldData.chunkSize * (y + WorldData.chunkSize * z)] = value;
    }

    void AddFaces(int x, int y, int z)
    {
        int thisBlockId = GetVoxel(x, y, z);
        Vector3 basePos = new Vector3(x, y, z);

        for (int f = 0; f < 6; f++)
        {
            Vector3Int dir = Block.faceDirs[f];

            int nx = x + dir.x;
            int ny = y + dir.y;
            int nz = z + dir.z;

            bool shouldDrawFace;

            if (IsVoxelInChunk(nx, ny, nz))
            {
                int neighbourId = GetVoxel(nx, ny, nz);
                // draw if neighbour is NOT solid (air)
                shouldDrawFace = !world.blockTypes.blockTypes[neighbourId].solid;
            }
            else
            {
                //Check which neighbour chunk we are looking at based on direction, left will be left chunk, right will be right chunk, etc. then check if that chunk is valid and if so check the voxel in that chunk at the correct position, if not valid then treat as air and draw face
                ChunkPosition neighbourChunk = new ChunkPosition(chunkPosition.x + dir.x, chunkPosition.y + dir.y, chunkPosition.z + dir.z);
                Debug.Log($"Checking neighbour chunk at {neighbourChunk.x}, {neighbourChunk.y}, {neighbourChunk.z}");
                if (world.IsChunkValid(neighbourChunk))
                {
                    Debug.Log("Neighbour chunk is valid");
                    //Convert our local position to the neighbour chunk's local position
                    int localX = (nx + WorldData.chunkSize) % WorldData.chunkSize;
                    int localY = (ny + WorldData.chunkSize) % WorldData.chunkSize;
                    int localZ = (nz + WorldData.chunkSize) % WorldData.chunkSize;

                    if (world.GetVoxel(new Vector3(localX, localY, localZ), neighbourChunk) == 0)
                    {
                        shouldDrawFace = true;
                    }
                    else
                    {
                        shouldDrawFace = false;
                    }
                }
                else
                {
                    shouldDrawFace = true;
                }

            }

            if (shouldDrawFace)
            {
                AppendFace(basePos, (Block.Face)f, thisBlockId);
            }
        }
    }


        void AppendFace(Vector3 position, Block.Face face, int blockId)
        {
            for (int i = 0; i < 4; i++)
            {
                vertices.Add(Block.voxelFaceVertices[(int)face, i] + position);

            }
            for (int i = 0; i < 6; i++)
            {
                triangles.Add(currentIndex + Block.voxelTris[i]);
            }

            for (int i = 0; i < 4; i++)
            {
                uvs.Add(new Vector3(Block.voxelUVs[i].x, Block.voxelUVs[i].y,
                world.blockTypes.blockTypes[blockId].atlasTiles[(int)face]));
            }
            currentIndex += 4;
        }
    }
