using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

//Class: Chunk class repsents a cubic chunk in the game world.

public class OldChunk
{
   /* World world;
    public GameObject gameObject;


    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;


    //private List<Vector3> vertices = new List<Vector3>();
    //private List<int> triangles = new List<int>();
    //private List<Vector3> uvs = new List<Vector3>();

    int currentIndex = 0;

    [SerializeField] private int[] chunkData;




    ChunkPosition chunkPosition;



    //Job Data
    private NativeArray<int> nativeChunkData;
    private JobHandle populateHandle;

    private bool populateScheduled;
    private bool populateFinalized;


    public JobHandle PopulateHandle => populateHandle;
    public bool IsPopulateFinalized => populateFinalized;

    //Mesh Data
    private NativeList<Vector3> vertices;
    private NativeList<int> triangles;
    private NativeList<Vector3> uvs;




    //Contstructor
    public Chunk(ChunkPosition _chunkPosition, World _world)
    {
        world = _world;

        gameObject = new();
        gameObject.transform.parent = world.transform;

        chunkPosition = _chunkPosition;
        Vector3 worldPos = new Vector3(chunkPosition.position.x * WorldData.chunkSize, chunkPosition.position.y * WorldData.chunkSize, chunkPosition.position.z * WorldData.chunkSize);
        gameObject.transform.position = worldPos;
        gameObject.name = $"Chunk: {chunkPosition.position.x}, {chunkPosition.position.y}, {chunkPosition.position.z}";

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.masterMaterial;

        mesh = new();
    }
    //---Refactored---
    public void PopulateChunk()
    {
        //Create the results
        nativeChunkData = new NativeArray<int>(WorldData.chunkSize * WorldData.chunkSize * WorldData.chunkSize, Allocator.Persistent);

        //Create the job
        PopulateChunkJob populateChunkJob = new PopulateChunkJob
        {
            chunkDataResult = nativeChunkData,
            chunkPos = chunkPosition,
            worldGen = world.worldGenerationSettings
        };

        //Schedule it
        populateHandle = populateChunkJob.Schedule();

        //Set states
        populateScheduled = true;
        populateFinalized = false;


    }

    public void FinalizePopulate()
    {
        if (!populateScheduled || populateFinalized)
        {
            return;
        }

        populateHandle.Complete();

        chunkData = nativeChunkData.ToArray();

        populateFinalized = true;
    }





    //Used once the chunk is created to generate and draw the chunk, delayed to allow for chunk population first and its neighbours
    public void DrawChunk()
    {
        if (!populateFinalized)
        {
            Debug.Log($"Tried to draw chunk before population finished: {chunkPosition.position}");
            return;
        }

        if (chunkData == null || chunkData.Length == 0)
        {
            Debug.LogError($"Chunk data is null/empty for chunk at position: {chunkPosition.position}");
            return;
        }
        //Reset Mesh Data
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        GenerateChunk();
        SetupMesh();
    }
    void SetupMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, uvs);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
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
    public int GetVoxel(int x, int y, int z)
    {
        if (chunkData == null || chunkData.Length == 0) { Debug.Log($"No Chunk Data for {chunkPosition.position}"); }
        //Read from the chunkVoxels array
        return chunkData[x + y * WorldData.chunkSize + z * WorldData.chunkSize * WorldData.chunkSize];
    }
    void SetVoxel(int x, int y, int z, int value)
    {
        //Set value in chunkVoxels array
        chunkData[x + WorldData.chunkSize * (y + WorldData.chunkSize * z)] = value;
    }

    //Todo: Cleanup & refactor this a bit as its a larger function, maybe split into multiple functions, also add comments
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
                ChunkPosition neighbourChunk = new ChunkPosition(chunkPosition.position + new int3(dir.x, dir.y, dir.z));
                if (world.TryGetChunk(neighbourChunk, out Chunk chunk))
                {
                    //Calculate local position in neighbour chunk
                    int localX = (nx + WorldData.chunkSize) % WorldData.chunkSize;
                    int localY = (ny + WorldData.chunkSize) % WorldData.chunkSize;
                    int localZ = (nz + WorldData.chunkSize) % WorldData.chunkSize;

                    int neighbourId = chunk.GetVoxel(localX, localY, localZ);
                    shouldDrawFace = !world.blockTypes.blockTypes[neighbourId].solid;
                }
                else
                {
                    //Chunk has not been generated yet so treat as air but dont face
                    shouldDrawFace = false;
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
[BurstCompile]
public struct PopulateChunkJob : IJob
{
    public NativeArray<int> chunkDataResult;
    public ChunkPosition chunkPos;
    public WorldGenerationSettings worldGen;
    public void Execute()
    {
        //Fill with full values
        for (int x = 0; x < WorldData.chunkSize; x++)
        {
            for (int y = 0; y < WorldData.chunkSize; y++)
            {
                for (int z = 0; z < WorldData.chunkSize; z++)
                {
                    float3 worldPosition = new float3(x, y, z) + (float3)chunkPos.position * WorldData.chunkSize;
                    chunkDataResult[x + y * WorldData.chunkSize + z * WorldData.chunkSize * WorldData.chunkSize] = worldGen.GetVoxel(worldPosition);
                }
            }
        }
    }
}
public struct BuildMeshJob : IJob
{
    public World world;
    public ChunkPosition chunkPosition;
    public NativeArray<int> chunkData; //Used to generate 
    private NativeList<Vector3> vertices;
    private NativeList<int> triangles;
    private NativeList<Vector3> uvs;
    private int currentIndex;

    //Returns the voxel data for that position
    public int GetVoxel(int3 voxelPosition)
    {
        if (chunkData == null || chunkData.Length == 0) { Debug.Log($"No Chunk Data for Chunk: {chunkPosition.position}"); }
        //Read from the chunkVoxels array
        return chunkData[voxelPosition.x + voxelPosition.y * WorldData.chunkSize + voxelPosition.z * WorldData.chunkSize * WorldData.chunkSize];
    }
    bool IsVoxelInChunk(int3 voxelPosition)
    {
        if (voxelPosition.x < 0 || voxelPosition.x >= WorldData.chunkSize ||
        voxelPosition.y < 0 || voxelPosition.y >= WorldData.chunkSize ||
         voxelPosition.z < 0 || voxelPosition.z >= WorldData.chunkSize)
        {
            return false;
        }
        return true;
    }

    void AppendFace(int3 position, Block.Face face, int blockId)
    {
        for (int i = 0; i < 4; i++)
        {
            vertices.Add(Block.voxelFaceVertices[(int)face, i] + new Vector3(position.x, position.y, position.z));

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

    void AddFaces(int3 voxelPosition)
    {
        int thisBlockId = GetVoxel(voxelPosition);
        int3 basePos = voxelPosition;

        for (int f = 0; f < 6; f++)
        {
            Vector3Int dir = Block.faceDirs[f];

            int nx = voxelPosition.x + dir.x;
            int ny = voxelPosition.y + dir.y;
            int nz = voxelPosition.z + dir.z;

            bool shouldDrawFace;

            int3 checkPosition = new int3(nx, ny, nz);
            if (IsVoxelInChunk(checkPosition))
            {
                int neighbourId = GetVoxel(checkPosition);
                // draw if neighbour is NOT solid (air)
                shouldDrawFace = !world.blockTypes.blockTypes[neighbourId].solid;
            }
            else
            {
                //Check which neighbour chunk we are looking at based on direction, left will be left chunk, right will be right chunk, etc. then check if that chunk is valid and if so check the voxel in that chunk at the correct position, if not valid then treat as air and draw face
                ChunkPosition neighbourChunk = new ChunkPosition(chunkPosition.position + new int3(dir.x, dir.y, dir.z));
                if (world.TryGetChunk(neighbourChunk, out Chunk chunk))
                {
                    //Calculate local position in neighbour chunk
                    int localX = (nx + WorldData.chunkSize) % WorldData.chunkSize;
                    int localY = (ny + WorldData.chunkSize) % WorldData.chunkSize;
                    int localZ = (nz + WorldData.chunkSize) % WorldData.chunkSize;

                    int neighbourId = chunk.GetVoxel(localX, localY, localZ);
                    shouldDrawFace = !world.blockTypes.blockTypes[neighbourId].solid;
                }
                else
                {
                    //Chunk has not been generated yet so treat as air but dont face
                    shouldDrawFace = false;
                }

            }

            if (shouldDrawFace)
            {
                AppendFace(basePos, (Block.Face)f, thisBlockId);
            }
        }
    }




    public void Execute()
    {
        //Clear all mesh data to begin with
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        currentIndex = 0;
        for (int x = 0; x < WorldData.chunkSize; x++)
        {
            for (int y = 0; y < WorldData.chunkSize; y++)
            {
                for (int z = 0; z < WorldData.chunkSize; z++)
                {
                    int3 voxelPosition = new int3(x, y, z);
                    //Check if it is airblock or not then gen faces
                    if (GetVoxel(voxelPosition) == 0) continue;
                    //Append Faces
                    AddFaces(voxelPosition);

                }
            }
        }
    }*/
}
