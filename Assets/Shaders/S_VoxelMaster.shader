//Credit to Hollistic3D on YT for texture array sampling code
Shader "Voxel/S_VoxelMaster"
{
    //Where values are passed through from inspector to shader
    Properties
    {
        _MainTex ("Albedo=", 2DArray) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert
        UNITY_DECLARE_TEX2DARRAY(_MainTex);
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            //W Value for accessing different texture in array
            float arrayIndex;
        };


        void vert(inout appdata_full v, out Input o)
        {
            o.uv_MainTex = v.texcoord.xy;
            o.arrayIndex = v.texcoord.z;
        }

        void surf(Input IN, inout SurfaceOutputStandard o){
            fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(IN.uv_MainTex, IN.arrayIndex));
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
