//This holds the data for a block so we can add it to the mesh
using UnityEngine;
public class Block
{
    public static Vector3[] vertices =
    {
        //Front
        new Vector3(0,0,0), //0
        new Vector3(0,1,0), //1
        new Vector3(1,0,0), //2
        new Vector3(1,1,0), //3
        //Back
        new Vector3(0,0,1), //4
        new Vector3(0,1,1), //5
        new Vector3(1,0,1), //6
        new Vector3(1,1,1)  //7
    };
    public static int[] indices =
    {
        0,1,2, 2,1,3, //Front face
        4,5,0, 0,5,1, // Left Face
        6,2,3, 3,7,6, //Right Face
        6,7,4, 4,7,5, //Back Face
        1,5,7, 7,3,1, //Top Face
        6,4,0, 0,2,6 //Bottom Face

    };
    //All faces aranged in a clockwise rotation
    //Starting from bottom left corner to bottom right according to forward dir
    public static Vector3[,] faceVertices =
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

    public static int[] voxelTris =  {0,1,3,3,1,2};
}