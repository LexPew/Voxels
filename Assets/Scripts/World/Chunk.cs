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
    public int3 chunkPosition;

    //Chunk Data
    NativeArray<int> chunkData;

    private int dirtyMask = 0; //Bitmask to track which faces need updating, 1 for each face so 6 bits total, if a bit is 1 it means that face needs to be updated
    public Chunk[] neighbours = new Chunk[6]; //Array to hold references to our 6 neighbours, order is front, back, left, right, top, bottom

    //Mesh Data
    NativeList<float3> vertices;
    NativeList<int> triangles;
    NativeList<float3> uvs;
    NativeList<float3> normals;
    int currentIndex = 0;

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

    public void RecieveNeighbourChange(Chunk neighbour, int faceDir)
    {
        //Bitwise OR the dirtyMask with a bit shifted by the faceDir to mark that face as dirty
        //So for the front face we should shift 1 by 0, for the back face we shift 1 by 1 and so on, this way we can track which faces need updating with a single integer
        //E.g 0000001 means the front face is dirty, 0000 010 means the back face is dirty, 0000 011 means both the front and back faces are dirty and so on
        dirtyMask |= (1 << faceDir);
        //I love bits :D
    }




    //Informs neighbours our state has changed
  /*  public void InformNeighbours()
    {
        //Loop through all six neighbours and inform them of us changing 
        for (int i = 0; i < 6; i++)
        {
            //Try get neighbours if they exist
            int3 neighbourPosition = chunkPosition + Block.faceDirs[i];

            //Uses the faceDirs from the block class as they represent the 6 cardinal directions,


            //Try get it and inform
            world.TryGetChunk(neighbourPosition, out Chunk neighbour);
            if (neighbour != null)
            {
                neighbour.RecieveNeighbourChange(this, i);
            }
        }
    }

    public void RecieveNeighbourChange(Chunk neighbour, int faceDir)
    {
        //Now we have this info we need to get the border slice from the neighbour and update our slice at the border aswell


        int neighbourStartX = 0;
        int neighbourEndX = WorldData.chunkSize;

        int neighbourStartZ = 0;
        int neighbourEndZ = WorldData.chunkSize;

        int neighbourStartY = 0;
        int neighbourEndY = WorldData.chunkSize;

        int startX = 0;
        int endX = WorldData.chunkSize;

        int startY = 0;
        int endY = WorldData.chunkSize;

        int startZ = 0;
        int endZ = WorldData.chunkSize;
        //We can use the faceDir to determine where the neighbour slice starts when we extract data
        switch (faceDir)
        {
            case 0: //Front Face -z
                neighbourStartZ = WorldData.chunkSize - 1;
                neighbourEndZ = WorldData.chunkSize;

                startZ = 0;
                endZ = 1;
                break;
            case 1: //Back Face +z
                neighbourStartZ = 0;
                neighbourEndZ = 1;

                startZ = WorldData.chunkSize - 1;
                endZ = WorldData.chunkSize;
                break;
            case 2: //Left Face -x
                neighbourStartX = 0;
                neighbourEndX = 1;

                startX = WorldData.chunkSize - 1;
                endX = WorldData.chunkSize;
                break;
            case 3: //Right Face +x
                neighbourStartX = WorldData.chunkSize - 1;
                neighbourEndX = WorldData.chunkSize;

                startX = 0;
                endX = 1;
                break;
            case 4: //Top Face +y
                neighbourStartY = WorldData.chunkSize - 1;
                neighbourEndY = WorldData.chunkSize;

                startY = 0;
                endY = 1;
                break;
            case 5: //Bottom Face -y
                neighbourStartY = 0;
                neighbourEndY = 1;

                startY = WorldData.chunkSize - 1;
                endY = WorldData.chunkSize;
                break;


        }
        //Need an array size of a slice so size chunksi*chunkSize, this way its just the out border slice of the chunk
        int[] neighbourSlice = new int[WorldData.chunkSize * WorldData.chunkSize];
        for (int x = neighbourStartX; x < neighbourEndX; x++)
        {
            for (int z = neighbourStartZ; z < neighbourEndZ; z++)
            {
                for (int y = neighbourStartY; y < neighbourEndY; y++)
                {
                    int index = x + y * WorldData.chunkSize + z * WorldData.chunkSize * WorldData.chunkSize;
                    //Data will be put into the array in order from bottom left to top right so we can just loop through and add them in order
                    neighbourSlice[x + y * WorldData.chunkSize] = neighbour.chunkData[index];
                }
            }
        }

        //Slow remesh for now no jobs, just update the border data and resend to the mesh
        //So loop through the slice and compare to the neighbour then if need be remove and append faces appropriate faces
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int z = startZ; z < endZ; z++)
                {
                    //By default we dont generate chunk borders so we just need to check whether to add faces and setmesh
                    int neighBourIndex = x + y * WorldData.chunkSize;
                    int neighbourID = neighbourSlice[neighBourIndex];
                    int ourIndex = x + y * WorldData.chunkSize + z * WorldData.chunkSize * WorldData.chunkSize;
                    int ourID = chunkData[ourIndex];
                    bool shouldHaveFace = (neighbourID == 0);
                    bool hasFace = (ourID != 0);
                    if (shouldHaveFace && !hasFace)
                    {
                        //We need to add a face here as the neighbour has air but we dont
                        AppendFace(new int3(x, y, z), (Block.Face)faceDir, ourID);
                    }
                    else if (!shouldHaveFace && hasFace)
                    {
                        //Add later
                    }
                }
            }
        }
        SetMesh();
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
             world.blockTypesBurst[blockId].atlasTiles[(int)face]));
        }
        for (int i = 0; i < 4; i++)
        {
            normals.Add(Block.voxelFaceNormals[(int)face]);
        }
        currentIndex += 4;
    }*/

    //Creates the Mesh and Draws the chunk
    public void Mesh()
    {
        mesh.Clear();

        vertices = new NativeList<float3>(Allocator.Persistent);
        triangles = new NativeList<int>(Allocator.Persistent);
        uvs = new NativeList<float3>(Allocator.Persistent);
        normals = new NativeList<float3>(Allocator.Persistent);
        currentIndex = 0;
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
    public void SetMesh()
    {
        mesh.SetVertices(vertices.AsArray());
        mesh.SetIndices(triangles.AsArray(), MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs.AsArray());
        mesh.SetNormals(normals.AsArray());

        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }
    IEnumerator MeshThread()
    {
        while (!buildMeshHandle.IsCompleted)
        {
            yield return null;
        }
        // Ensure the job is completed before accessing NativeList data
        buildMeshHandle.Complete();

        SetMesh();
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
                    shouldDrawFace = false;
                }

                if (shouldDrawFace)
                {
                    AppendFace(voxelPosition, (Block.Face)f, blockId);
                }
            }
        }
    }
}
