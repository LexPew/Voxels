using System.Collections;
using System.Collections.Generic;
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

    [ContextMenu("Gen")]
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
}
