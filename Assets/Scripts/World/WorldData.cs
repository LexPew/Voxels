using Unity.Collections;

public class WorldData //Convert to scriptable object at some point
{
    public const int chunkSize = 16;
    public const int worldSizeInChunks = 8;
    public const int worldHeightInChunks = 8;

    public static int WorldSizeInVoxelsX => worldSizeInChunks * chunkSize;
    public static int WorldSizeInVoxelsY => worldHeightInChunks * chunkSize;
    public static int WorldSizeInVoxelsZ => worldSizeInChunks * chunkSize;

}