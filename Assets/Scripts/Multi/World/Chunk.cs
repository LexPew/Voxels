using System.Collections;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk
{
    //Mono
    World world;
    public GameObject gameObject;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    //Chunks position
    int3 chunkPosition;

    //Chunk Data
    NativeArray<int> chunkData;

    //Mesh Data
    NativeList<float3> vertices;
    NativeList<int> triangles;
    NativeList<float3> uvs;
    NativeList<float3> normals;

    //Job Data
    JobHandle buildMeshHandle;
    JobHandle populateHandle;

    bool populateScheduled;
    bool populateFinalized;

    public JobHandle PopulateHandle => populateHandle;
    public bool IsPopulateFinalized => populateFinalized;

    public Chunk(int3 _chunkPosition, World _world)
    {
        chunkPosition = _chunkPosition;
        world = _world;

        //Setup gameobject
        gameObject = new();
        gameObject.transform.parent = world.transform;
        gameObject.isStatic = true; //Chunks are static, we wont be moving them after creation so this should help performance
        //Setup name
        Vector3 worldPos = new Vector3(chunkPosition.x * WorldData.chunkSize, chunkPosition.y * WorldData.chunkSize, chunkPosition.z * WorldData.chunkSize);
        gameObject.transform.position = worldPos;
        gameObject.name = $"Chunk: {chunkPosition.x}, {chunkPosition.y}, {chunkPosition.z}";

        //Setup mesh & components
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.masterMaterial;

        mesh = new();
    }

    public void Dispose()
    {
        if (chunkData.IsCreated)
        {
            chunkData.Dispose();
        }
        if (vertices.IsCreated)
        {
            vertices.Dispose();
        }
        if (triangles.IsCreated)
        {
            triangles.Dispose();
        }
        if (uvs.IsCreated)
        {
            uvs.Dispose();
        }
        if (normals.IsCreated)
        {
            normals.Dispose();
        }
    }

    public void PopulateChunk()
    {
        //Create the results
        chunkData = new NativeArray<int>(WorldData.chunkSize * WorldData.chunkSize * WorldData.chunkSize, Allocator.Persistent);

        //Create the job
        PopulateChunkJob populateChunkJob = new PopulateChunkJob
        {
            chunkDataResult = chunkData,
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

        populateFinalized = true;
    }

    //Creates the Mesh and Draws the chunk
    public void Mesh()
    {
        mesh.Clear();

        vertices = new NativeList<float3>(Allocator.Persistent);
        triangles = new NativeList<int>(Allocator.Persistent);
        uvs = new NativeList<float3>(Allocator.Persistent);
        normals = new NativeList<float3>(Allocator.Persistent);

        BuildMesh buildMeshJob = new BuildMesh
        {
            chunkData = chunkData,
            vertices = vertices,
            triangles = triangles,
            uvs = uvs,
            normals = normals,
            chunkSize = WorldData.chunkSize,
            blockDefBursts = world.blockTypesBurst
        };

        buildMeshHandle = buildMeshJob.Schedule();
        world.StartCoroutine(MeshThread());
    }
    IEnumerator MeshThread()
    {
        while (!buildMeshHandle.IsCompleted)
        {
            yield return null;
        }
        // Ensure the job is completed before accessing NativeList data
        buildMeshHandle.Complete();
        mesh.SetVertices(vertices.AsArray());
        mesh.SetIndices(triangles.AsArray(), MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs.AsArray());
        mesh.SetNormals(normals.AsArray());

        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        vertices.Dispose();
        triangles.Dispose();
        uvs.Dispose();
        normals.Dispose();
    }

    [BurstCompile]
    public struct PopulateChunkJob : IJob
    {
        public NativeArray<int> chunkDataResult;
        public int3 chunkPos;
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
                        float3 worldPosition = new float3(x, y, z) + (float3)chunkPos * WorldData.chunkSize;
                        chunkDataResult[x + y * WorldData.chunkSize + z * WorldData.chunkSize * WorldData.chunkSize] = worldGen.GetVoxel(worldPosition);
                    }
                }
            }
        }
    }





    //TODO: Change the block.cs as theyre not burst compat
    public struct BuildMesh : IJob
    {

        [ReadOnly] public NativeArray<int> chunkData;

        public NativeList<float3> vertices;
        public NativeList<int> triangles;
        public NativeList<float3> uvs;
        public NativeList<float3> normals;

        private int currentIndex;
        public int chunkSize;

        [ReadOnly] public NativeArray<BlockDefBurst> blockDefBursts;

        public void Execute()
        {
            currentIndex = 0;

            for (int x = 0; x < chunkSize; x++)
                for (int y = 0; y < chunkSize; y++)
                    for (int z = 0; z < chunkSize; z++)
                    {
                        //Voxels position
                        int3 voxelPosition = new int3(x, y, z);

                        //Check if it is an airblock, if not generate faces
                        if (GetVoxelID(voxelPosition) == 0)
                        {
                            continue; //AirBlock
                        }

                        //Add Faces to mesh data
                        AddFaces(voxelPosition);


                    }
        }

        //Returns the integer ID for the voxel in position if it exists
        private int GetVoxelID(int3 voxelPosition)
        {
            if (!chunkData.IsCreated || chunkData.Length == 0) { return 0; }
            //Read from the chunkVoxels array
            return chunkData[voxelPosition.x + voxelPosition.y * chunkSize + voxelPosition.z * chunkSize * chunkSize];
        }

        bool IsVoxelInChunk(int3 voxelPosition)
        {
            if (voxelPosition.x < 0 || voxelPosition.x >= chunkSize ||
            voxelPosition.y < 0 || voxelPosition.y >= chunkSize ||
             voxelPosition.z < 0 || voxelPosition.z >= chunkSize)
            {
                return false;
            }
            return true;
        }
        void AppendFace(int3 position, Block.Face face, int blockId)
        {
            float3 basePos = position;
            for (int i = 0; i < 4; i++)
            {
                vertices.Add(Block.voxelFaceVertices[(int)face, i] + basePos);

            }
            for (int i = 0; i < 6; i++)
            {
                triangles.Add(currentIndex + Block.voxelTris[i]);
            }

            for (int i = 0; i < 4; i++)
            {
                //TODO: Add uvs
                //uvs.Add(new Vector3(Block.voxelUVs[i].x, Block.voxelUVs[i].y,
                // world.blockTypes.blockTypes[blockId].atlasTiles[(int)face]));
                uvs.Add(new float3(Block.voxelUVs[i].x, Block.voxelUVs[i].y,
                 blockDefBursts[blockId].atlasTiles[(int)face]));
            }
            for (int i = 0; i < 4; i++)
            {
                normals.Add(Block.voxelFaceNormals[(int)face]);
            }
            currentIndex += 4;
        }
        private void AddFaces(int3 voxelPosition)
        {
            int blockId = GetVoxelID(voxelPosition);

            //Loop through and check each faces neighbour
            for (int f = 0; f < 6; f++)
            {
                int3 dir = Block.faceDirs[f];

                int nx = voxelPosition.x + dir.x;
                int ny = voxelPosition.y + dir.y;
                int nz = voxelPosition.z + dir.z;

                bool shouldDrawFace = false;

                int3 neighbourPosition = new int3(nx, ny, nz);


                //TODO: Add neighbour checking properly later
                if (IsVoxelInChunk(neighbourPosition))
                {
                    int neighbourID = GetVoxelID(neighbourPosition);

                    shouldDrawFace = (neighbourID == 0);
                }
                else
                {
                    shouldDrawFace = true;
                }

                if (shouldDrawFace)
                {
                    AppendFace(voxelPosition, (Block.Face)f, blockId);
                }
            }
        }
    }
}
