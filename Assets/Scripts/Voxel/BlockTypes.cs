using System;
using Unity.Burst;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct BlockDef
{
    //Block ID is the array index
    public string name;
    public bool solid;
    public bool gravity;
    [Tooltip("Front,Back,Left,Right,Top,Bottom")]
    public int[] atlasTiles;
}
[Serializable][BurstCompile]
public struct BlockDefBurst
{
    public FixedString32Bytes name;
    public bool solid;
    public bool gravity;
    public FixedList64Bytes<int> atlasTiles;
}

[CreateAssetMenu(fileName = "BlockTypes", menuName = "Voxel/BockTypes", order = 1)]
public class BlockTypes : ScriptableObject
{
    public BlockDef[] blockTypes;

    public NativeArray<BlockDefBurst> ToNative()
    {
        //Converts all block definitions to native array
        NativeArray<BlockDefBurst> blockDefs = new NativeArray<BlockDefBurst>(blockTypes.Length, Allocator.Persistent);

        for(int i = 0; i < blockTypes.Length; i++)
        {
            BlockDef currentDef = blockTypes[i];
            BlockDefBurst newDef = new BlockDefBurst
            {
                name = currentDef.name,
                solid = currentDef.solid,
                gravity = currentDef.gravity,
            };
            for(int j = 0; j < currentDef.atlasTiles.Length; j++)
            {
                newDef.atlasTiles.Add(currentDef.atlasTiles[j]);
            }
            blockDefs[i] = newDef;
        }

        return blockDefs;
    }
}
