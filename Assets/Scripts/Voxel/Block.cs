//This holds the data for a block so we can add it to the mesh
using UnityEngine;
public class Block
{
    public enum Face
    {
        Front = 0,
        Back = 1,
        Left = 2,
        Right = 3,
        Top = 4,
        Bottom = 5
    }
    //All faces aranged in a clockwise rotation
    //Starting from bottom left corner to bottom right according to forward dir
    public static readonly Vector3[,] faceVertices =
    {
        //Front Face
        {
            new Vector3(0,0,0),
            new Vector3(0,1,0),
            new Vector3(1,1,0),
            new Vector3(1,0,0)

        },
        //Back Face
        {
            new Vector3(1,0,1),
            new Vector3(1,1,1),
            new Vector3(0,1,1),
            new Vector3(0,0,1)
        },
        //Left Face
        {
            new Vector3(0,0,1),
            new Vector3(0,1,1),
            new Vector3(0,1,0),
            new Vector3(0,0,0)
        },
        //Right Face
        {
            new Vector3(1,0,0),
            new Vector3(1,1,0),
            new Vector3(1,1,1),
            new Vector3(1,0,1)
        },
        //Top Face
        {
            new Vector3(0,1,0),
            new Vector3(0,1,1),
            new Vector3(1,1,1),
            new Vector3(1,1,0)
        },
        //Bottom Face
        {
            new Vector3(0,0,0),
            new Vector3(1,0,0),
            new Vector3(1,0,1),
            new Vector3(0,0,1)
        },
    };

    public static readonly Vector3Int[] faceDirs =
    {
        new Vector3Int(0,0,-1), //Front -z
        new Vector3Int(0,0,1), //Back +z
        new Vector3Int(-1,0,0), //Left -x
        new Vector3Int(1,0,0), //Right +x
        new Vector3Int(0,1,0), //Top +y
        new Vector3Int(0,-1,0), //Top -y
    };

    public static int[] voxelTris = { 0, 1, 3, 3, 1, 2 };


}