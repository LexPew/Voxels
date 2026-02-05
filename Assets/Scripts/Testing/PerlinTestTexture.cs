using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PerlinTestTexture : MonoBehaviour
{
    //Image height and width
    [SerializeField] private int imageWidth = 1024;
    [SerializeField] private int imageHeight = 1024;

    [SerializeField] private float noiseScale = 1.0f;

    private Texture2D texture;
    private Renderer textureRenderer;
    private Color[] pix;

    void Start()
    {
        //Grab renderer
        textureRenderer = GetComponent<Renderer>();

        //Setup texture
        texture = new Texture2D(imageWidth, imageHeight);
        pix = new Color[texture.width * texture.height];
        textureRenderer.material.mainTexture = texture;
    }


    void CalculateNoise()
    {
        for (float y = 0; y < imageHeight; y++)
        {
            for (float x = 0; x < imageWidth; x++)
            {
                float xCoord = x / texture.width * noiseScale;
                float yCoord = y / texture.height * noiseScale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                pix[(int) y * texture.width + (int)x] = new Color(sample, sample, sample, 1.0f);
            }
        }
        texture.SetPixels(pix);
        texture.Apply();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Reset
            CalculateNoise();
        }
    }
}
