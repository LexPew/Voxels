using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class: Chunk class repsents a cubic chunk in the game world.
public class Chunk : MonoBehaviour
{
    [SerializeField] private GameObject voxelPrefab;
    //Change to static later
    [SerializeField] private int cubicSize = 16;


    public bool generateOnStart = true;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateChunk();
        }
    }

    [ContextMenu("Generate Chunk")]
    void GenerateChunk()
    {
        for (int x = 0; x < cubicSize; x++)
        {
            for (int y = 0; y < cubicSize; y++)
            {
                for (int z = 0; z < cubicSize; z++)
                {
                    GameObject newVoxel = Instantiate(voxelPrefab, new Vector3(x,y,z), Quaternion.identity, transform);
                    newVoxel.name = $"{x},{y},{z}";
                }
            }
        }
    }

}
