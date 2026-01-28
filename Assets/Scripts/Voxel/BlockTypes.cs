using System;
using System.Collections;
using System.Collections.Generic;
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


[CreateAssetMenu(fileName = "BlockTypes", menuName = "Voxel/BockTypes", order = 1)]
public class BlockTypes : ScriptableObject
{
    public BlockDef[] blockTypes;
}
