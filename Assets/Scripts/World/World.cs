using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class World : MonoBehaviour
{
    //Data
    public Material masterMaterial;
    public BlockTypes blockTypes;

    private Chunk[,,] chunks = new Chunk[WorldData.worldSizeInChunks, WorldData.worldHeightInChunks, WorldData.worldSizeInChunks];
    void Start()
    {
        PopulateChunks();
        DrawChunks();
    }

    void PopulateChunks()
    {
        for (int x = 0; x < WorldData.worldSizeInChunks; x++)
        {
            for (int y = 0; y < WorldData.worldHeightInChunks; y++)
            {
                for (int z = 0; z < WorldData.worldSizeInChunks; z++)
                {
                    CreateChunk(new Vector3Int(x,y,z));
                }
            }
        }
    }

    void DrawChunks()
    {
        foreach (Chunk chunk in chunks)
        {
            chunk.DrawChunk();
        }
    }

    void CreateChunk(Vector3Int position)
    {
        //Will be used at some point to update neighbours walls 
        Chunk newChunk = new(position, this);
        chunks[position.x, position.y, position.z] = newChunk;
    }

    //Helper functions
    bool IsChunkInWorld(Vector3 position)
    {
        if(position.x > 0 && position.x < WorldData.worldSizeInChunks - 1
            && position.y > 0 && position.y < WorldData.worldHeightInChunks - 1 
            && position.z > 0 && position.z < WorldData.worldSizeInChunks - 1)
        {
            return true;
        }
        return false;
    }

    bool IsVoxelInWorld(Vector3 position)
    {
        if(position.x >= 0 && position.x < WorldData.WorldSizeInVoxelsX
            && position.y >= 0 && position.y < WorldData.WorldSizeInVoxelsY 
            && position.z >= 0 && position.z < WorldData.WorldSizeInVoxelsZ)
        {
            return true;
        }
        return false;
    }


    //New Method for grabbing voxels so i can implement perlin, biomes, etc
    //Will be called by chunks when populating
    public int GetVoxel(Vector3 position)
    {
        //TODO: Replace with better system, perlin noise & SO for each biome maybe a registry like the block types
        if (position.y == WorldData.chunkSize - 1 && Random.Range(0, 100) >= 20)
        {
            return 1;
        }
        else if (position.y == WorldData.chunkSize - 1)
        {
            return 0;
        }
        else if (position.y <= WorldData.chunkSize - 1 && position.y >= WorldData.chunkSize - 4 && Random.Range(0, 100) >= 50)
        {
            return 2;
        }
        else if (Random.Range(0, 100) >= 20)
        {
            return 3;
        }
        else
        {
            return 0;
        }

    }

}

