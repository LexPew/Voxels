//Job and Burst compatible world generator
using System;
using Unity.Burst;
using Unity.Mathematics;

//Struct: Used to generate the world, currently just has a method to get the noise height at a given position but will likely have more in the future, using static methods and variables as we dont need multiple instances of this and it allows us to easily call it from other classes without needing a reference to an instance of the world generator
[Serializable]
public struct WorldGenerationSettings
{
    public float noiseScale;
    public float heightAmplification;

    public float seed;
    WorldGenerationSettings(float _noiseScale, float _heightAmplification, float _seed)
    {
        seed = _seed;
        noiseScale = _noiseScale;
        heightAmplification = _heightAmplification;
    }

    //Helper functions
    float GetNoiseHeight(float2 worldPosition)
    {
        //Have to remap the noise output from -1 to 1 to 0 to 1
        float sample = (noise.cnoise(worldPosition / noiseScale + seed) + 1) / 2 * heightAmplification;
        return sample;
    }

    public int GetVoxel(float3 worldPosition)
    {
        float noiseHeight = GetNoiseHeight(worldPosition.xz);
        if (worldPosition.y <= noiseHeight)
        {
            if(worldPosition.y == (int)noiseHeight)
            {
                return 1; //Grass Block
            }

            if(worldPosition.y <= noiseHeight * 0.85f)
            {
                return 3; //Stone Block
            }
            else if (worldPosition.y < noiseHeight * 0.95f)
            {
                return 2; //Dirt Block
            }
  
            return 2; //Grass Block
        }
        return 0; //Air
    }
}