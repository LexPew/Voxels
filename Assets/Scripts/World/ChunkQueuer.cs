using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
//Used to manage chunk loading and unloading 

//IMPROVEMENTS:
//Could batch a couple chunks at a time if possible and do them via combined jobs
//Base how many chunks to do on free job space / processing power or something
public class ChunkQueuer
{

    //Performance Metrics
    static float targetFrameTime = 1 / 60.0f; //We want a min 60fps frame time
    float frameTimeGive = targetFrameTime * 0.1f;
    public int chunksPerFrame = 0;
    private int maxChunksPerframe;


    public Queue<Chunk> chunksToPopulate = new Queue<Chunk>();

    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    public int chunksLeft => chunksToDraw.Count + chunksToPopulate.Count;
    public ChunkQueuer(int jobWorkerCount)
    {
        maxChunksPerframe = jobWorkerCount * 2;
    }

    public void QueueChunk(Chunk chunkToLoad)
    {
        chunksToPopulate.Enqueue(chunkToLoad);
    }

    public void Update(float deltaTime)
    {
        //Dynamically Update How many chunks we process per frame
        if (deltaTime <= targetFrameTime + frameTimeGive && chunksPerFrame < maxChunksPerframe)
        {
            chunksPerFrame++;
        }
        else if (deltaTime > targetFrameTime + frameTimeGive)
        {
            chunksPerFrame = Mathf.Max(1, chunksPerFrame - 1);
        }
    }



    //Need to make sure this doesnt stop main thread so we will load chunks at a pace determined by performance metrics,
    //for now we will just load 1 chunk per frame
    public void PopulateChunks()
    {
        if (chunksToPopulate.Count > 0)
        {
            for (int i = 0; i < Mathf.Min(chunksToPopulate.Count, chunksPerFrame); i++)
            {
                Chunk nextChunkInQueue = chunksToPopulate.Dequeue();

                nextChunkInQueue.PopulateChunk();
                chunksToDraw.Enqueue(nextChunkInQueue);
            }
        }
    }

    public void DrawChunks()
    {
        if (chunksToDraw.Count > 0)
        {
            for (int i = 0; i < Mathf.Min(chunksToDraw.Count, chunksPerFrame); i++)
            {
                Chunk nextChunkInQueue = chunksToDraw.Peek();
                if (nextChunkInQueue.PopulateHandle.IsCompleted)
                {
                    nextChunkInQueue = chunksToDraw.Dequeue();
                    nextChunkInQueue.FinalizePopulate();
                    nextChunkInQueue.Mesh();
                }
                else
                {
                    Chunk reQueue = chunksToDraw.Dequeue();
                    QueueChunk(reQueue);
                }

            }
        }
    }
}
