using System.Collections.Generic;
using UnityEngine;
//Used to manage chunk loading and unloading 

//IMPROVEMENTS:
//Could batch a couple chunks at a time if possible and do them via combined jobs
//Base how many chunks to do on free job space / processing power or something
public class ChunkQueuer
{
    public Queue<Chunk> chunksToPopulate = new Queue<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
    public void QueueChunk(Chunk chunkToLoad)
    {
        chunksToPopulate.Enqueue(chunkToLoad);
    }

    //Need to make sure this doesnt stop main thread so we will load chunks at a pace determined by performance metrics,
    //for now we will just load 1 chunk per frame
    public void PopulateChunks()
    {
        if (chunksToPopulate.Count > 0)
        {
            Chunk nextChunkInQueue = chunksToPopulate.Dequeue();

            nextChunkInQueue.PopulateChunk();

            chunksToDraw.Enqueue(nextChunkInQueue);
        }
    }

    public void DrawChunks()
    {
        if(chunksToDraw.Count > 0)
        {
            Chunk nextChunkInQueue = chunksToDraw.Dequeue();
            nextChunkInQueue.PopulateHandle.Complete();

            nextChunkInQueue.FinalizePopulate();
            nextChunkInQueue.Mesh();
        }
    }
}
