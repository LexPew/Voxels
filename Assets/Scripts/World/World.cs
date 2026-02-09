using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


public class World : MonoBehaviour
{
    //Data
    
    public Material masterMaterial;
    public BlockTypes blockTypes;
    public NativeArray<BlockDefBurst> blockTypesBurst;
    [SerializeField] private Dictionary<int3, Chunk> chunkDictionary = new Dictionary<int3, Chunk>();

    //New world generator, burst comptable
    public WorldGenerationSettings worldGenerationSettings;
    void Start()
    {
        //Setup
        blockTypesBurst = blockTypes.ToNative();


        PopulateChunks();
        StartCoroutine(DrawChunks());
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

    void PopulateChunks()
    {


        for (int x = 0; x < WorldData.worldSizeInChunks; x++)
        {
            for (int y = 0; y < WorldData.worldHeightInChunks; y++)
            {
                for (int z = 0; z < WorldData.worldSizeInChunks; z++)
                {
                    CreateChunk(new int3(x, y, z)).PopulateChunk();
                }
            }
        }


    }
    IEnumerator DrawChunks()
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


    }


    Chunk CreateChunk(int3 chunkPosition)
    {
        //Will be used at some point to update neighbours walls 
        Chunk newChunk = new Chunk(chunkPosition, this);
        chunkDictionary.Add(chunkPosition, newChunk);

        return newChunk;
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

   /*public int GetVoxelInChunk(Vector3Int position, int3 chunkPosition)
    {
        if (TryGetChunk(chunkPosition, out ChunkMulti chunk))
        {
            return chunk.GetVoxel(position.x, position.y, position.z);
        }
        else
        {
            //If chunk is not valid then treat as air
            return 0;
        }
    }*/
}

