//This holds the data for a block so we can add it to the mesh
using Unity.Mathematics;
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
    //All faces arranged in a clockwise rotation
    //Starting from bottom left corner to bottom right according to forward dir
    //TODO: Make this burst compatible via static arrays and indices
    public static readonly float3[,] voxelFaceVertices =
    {
        //Front Face
        {
            new float3(0,0,0),
            new float3(0,1,0),
            new float3(1,1,0),
            new float3(1,0,0)

        },
        //Back Face
        {
            new float3(1,0,1),
            new float3(1,1,1),
            new float3(0,1,1),
            new float3(0,0,1)
        },
        //Left Face
        {
            new float3(0,0,1),
            new float3(0,1,1),
            new float3(0,1,0),
            new float3(0,0,0)
        },
        //Right Face
        {
            new float3(1,0,0),
            new float3(1,1,0),
            new float3(1,1,1),
            new float3(1,0,1)
        },
        //Top Face
        {
            new float3(0,1,0),
            new float3(0,1,1),
            new float3(1,1,1),
            new float3(1,1,0)
        },
        //Bottom Face
        {
            new float3(0,0,0),
            new float3(1,0,0),
            new float3(1,0,1),
            new float3(0,0,1)
        },
    };

    public static int[] voxelTris = { 0, 1, 3, 3, 1, 2 };

    public static readonly float2[] voxelUVs =
    {
        new float2(0,0),
        new float2(0,1),
        new float2(1,1),
        new float2(1,0)
    };

    public static readonly int3[] faceDirs =
    {
        new int3(0,0,-1), //Front -z
        new int3(0,0,1), //Back +z
        new int3(-1,0,0), //Left -x
        new int3(1,0,0), //Right +x
        new int3(0,1,0), //Top +y
        new int3(0,-1,0), //Bottom -y
    };

    public static float3[] voxelFaceNormals =
    {
        new float3(0,0,-1), //Front
        new float3(0,0,1), //Back
        new float3(-1,0,0), //Left
        new float3(1,0,0), //Right
        new float3(0,1,0), //Top
        new float3(0,-1,0), //Bottom
    };


}