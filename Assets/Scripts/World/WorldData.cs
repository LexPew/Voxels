public class WorldData //Convert to scriptable object at some point
{
    public static int chunkSize = 16;
    public static int worldSizeInChunks = 12;
    public static int worldHeightInChunks = 4;

    public static int WorldSizeInVoxelsX => worldSizeInChunks * chunkSize;
    public static int WorldSizeInVoxelsY => worldHeightInChunks * chunkSize;
    public static int WorldSizeInVoxelsZ => worldSizeInChunks * chunkSize;

}