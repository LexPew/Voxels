using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


public class World : MonoBehaviour
{
    //Player Position used for chunk loading and unloading
    public Transform playerTransform;
    //Render distance has to be an even number
    public int renderDistance = 16;
    //Data

    public Material masterMaterial;
    public BlockTypes blockTypes;
    public NativeArray<BlockDefBurst> blockTypesBurst;
    //Will replace dictionary with a 3d array at some point the size of max view distance in chunks
    [SerializeField] private Dictionary<int3, Chunk> chunkDictionary = new Dictionary<int3, Chunk>();
    [SerializeField] private List<Chunk> chunksToUpdate = new List<Chunk>();

    ChunkQueuer chunker;


    //New world generator, burst comptable
    public WorldGenerationSettings worldGenerationSettings;
    void Awake()
    {
        chunker = new ChunkQueuer(Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerMaximumCount);
        //Setup
        blockTypesBurst = blockTypes.ToNative();
    }
    void Start()
    {
        Spawn();
        //StartCoroutine(DrawChunks());

        //Test chunk 0 neighbour positions
        TryGetChunk(int3.zero, out Chunk chunk);
    }

    void Update()
    {
        chunker.Update(Time.deltaTime);
        chunker.PopulateChunks();
    }
    void LateUpdate()
    {
        chunker.DrawChunks();
    }

    //OnSpawn draw a grid of chunks around the player
    void Spawn()
    {
        //Set the player spawn pos
        playerTransform.position = GetSpawnPosition();
        //Disable them until we have some chunks loaded around them so they dont fall through the world
        playerTransform.gameObject.SetActive(false);
        //Get Players Position in XZ
        int2 position = PositionToChunkGrid(playerTransform.position);
        int halfRender = renderDistance / 2;
        int startX = position.x - halfRender;
        int startZ = position.y - halfRender; //Y represents Z

        //Spawn some initial chunks
        for (int x = startX; x < startX + renderDistance; x++)
        {
            for (int z = startZ; z < startZ + renderDistance; z++)
            {
                for (int y = 0; y < WorldData.worldHeightInChunks; y++)
                {
                    CreateChunk(new int3(x, y, z));
                }

            }
        }
        StartCoroutine(EnablePlayerWhenReady());
    }

    void OnDestroy()
    {
        if (blockTypesBurst.IsCreated)
        {
            blockTypesBurst.Dispose();
        }
        foreach (var chunk in chunkDictionary.Values)
        {
            chunk.Dispose();
        }
    }


    //This creates a chunk and then also checks for neighbours and updates their walls, via a remesh function in the chunk class.
    //We also check if there is any chunks neighbouring this to set a flag for whether we need to update the chunk walls later.

    Chunk CreateChunk(int3 chunkPosition)
    {
        //Will be used at some point to update neighbours walls 
        Chunk newChunk = new Chunk(chunkPosition, this);
        chunkDictionary.Add(chunkPosition, newChunk);
        chunker.QueueChunk(newChunk);

        //Check for any neighbours and link them
        EstablishNeighbourLinks(newChunk, chunkPosition);
        return newChunk;
    }

    
    void EstablishNeighbourLinks(Chunk chunk, int3 chunkPosition)
    {
        //Loop through and establish all neighbouring chunks and link them if possible
        for (int i = 0; i < 6; i++)
        {
            int3 neighbourPosition = chunkPosition + Block.faceDirs[i];
            TryGetChunk(neighbourPosition, out Chunk neighbour);
            if (neighbour != null)
            {
                LinkNeighbours(chunk, neighbour, i);
            }
        }
    }

    //Links chunkA to chunkB in the direction of faceDir, so if faceDir is 0 (front) we will link to chunkB back
    void LinkNeighbours(Chunk chunkA, Chunk chunkB, int faceDir)
    {
        chunkA.neighbours[faceDir] = chunkB;
        chunkB.neighbours[faceDir ^ 1] = chunkA;

        //Using XOR operator (if both differ == 1) to reverse them with 1 
        //Front To Back
        ///000 000
        ///000 001
        //Back to Front
        ///000 001
        ///000 001
        ///000 000
        //Left To Right
        ///000 010
        ///000 001
        ///000 011
        ///ETC
    }
    //Chunk Border Functions
    public bool IsChunkValid(int3 chunkPosition)
    {
        if (chunkDictionary.ContainsKey(chunkPosition))
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    //Helper functions
    public bool TryGetChunk(int3 chunkPosition, out Chunk chunk)
    {
        return chunkDictionary.TryGetValue(chunkPosition, out chunk);
    }

    public int2 PositionToChunkGrid(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / WorldData.chunkSize);
        int z = Mathf.FloorToInt(position.z / WorldData.chunkSize);
        return new int2(x, z);
    }

    public Vector3 GetSpawnPosition()
    {
        Vector3 spawnPos = new Vector3(0, 0, 0);
        int index = WorldData.worldHeightInChunks * WorldData.chunkSize;
        while (!blockTypesBurst[worldGenerationSettings.GetVoxel(new float3(spawnPos.x, index, spawnPos.z))].solid)
        {
            index--;
        }
        spawnPos.y = index + 2;
        return spawnPos;
    }

    IEnumerator EnablePlayerWhenReady()
    {
        while (chunker.chunksLeft > 0)
        {
            yield return null;
        }
        playerTransform.gameObject.SetActive(true);
    }










    /*    IEnumerator DrawChunks()
        {
            //Combine all chunk jobs
            JobHandle combined = default;

            foreach (var chunk in chunkDictionary.Values)
            {
                combined = JobHandle.CombineDependencies(combined, chunk.PopulateHandle);
            }

            while (!combined.IsCompleted)
            {
                yield return null;
            }

            combined.Complete();

            foreach (Chunk chunk in chunkDictionary.Values)
            {
                chunk.FinalizePopulate();
            }

            foreach (Chunk chunk in chunkDictionary.Values)
            {
                chunk.Mesh();
            }


        }*/
}

