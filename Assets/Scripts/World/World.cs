using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


//Struct: Used to easily pass chunk positions around, also used as a key for dictionaries when i implement them

public struct ChunkPosition
{
    public int x;
    public int y;
    public int z;

    public ChunkPosition(int _x, int _y, int _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
}
public class World : MonoBehaviour
{
    //Data
    public Material masterMaterial;
    public BlockTypes blockTypes;

    [SerializeField] private Dictionary<ChunkPosition, Chunk> chunkDictionary = new Dictionary<ChunkPosition, Chunk>();


    //Perlin Noise
    [SerializeField] float noiseScale = 80.0f;
    [SerializeField] float heightAmplification = 10.0f;
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
                    CreateChunk(new ChunkPosition(x,y,z));
                }
            }
        }
    }

    void DrawChunks()
    {
        foreach(Chunk chunk in chunkDictionary.Values)
        {
            chunk.DrawChunk();
        }
    }

    void CreateChunk(ChunkPosition chunkPosition)
    {
        //Will be used at some point to update neighbours walls 
        Chunk newChunk = new(chunkPosition, this);
        chunkDictionary.Add(chunkPosition, newChunk);
    }

    //Chunk Border Functions
    public bool IsChunkValid(ChunkPosition chunkPosition)
    {
        if(chunkDictionary.ContainsKey(chunkPosition))
        {
            return true;
        }
        else
        {
            return false;
        }
    }







    //Helper functions
    public int GetVoxel(Vector3 position, ChunkPosition chunkPosition)
    {
        //Use perlin noise
        Vector3 blockOffset = new Vector3(chunkPosition.x, chunkPosition.y, chunkPosition.z) * WorldData.chunkSize;
        blockOffset += position;

        
        if(blockOffset.y <= SampleNoise(blockOffset))
        {
            return 1;
        }
        else
        {
            return 0;
        }

    }

    float SampleNoise(Vector3 position)
    {     
           float xCoord = position.x / noiseScale;
        float yCoord = position.z / noiseScale;
        float sample = Mathf.PerlinNoise(xCoord,yCoord) * heightAmplification;

        return sample;
    }

}

