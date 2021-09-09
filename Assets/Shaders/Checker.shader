Shader "Custom/Checker"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ColorB ("Color B", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0


        struct Input
        {
			fixed3 worldPos;
			fixed3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _ColorB;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			fixed xMod = abs(round(IN.worldPos.x))%2;
			fixed zMod = abs(round(IN.worldPos.z))%2;
			fixed checker = abs((xMod+zMod)-1);
			fixed4 c = lerp(_Color,lerp(_Color,_ColorB,checker),abs(IN.worldNormal.y));
			//fixed xMod = step(0,abs(round(IN.worldPos.x))%2-1);
			//fixed zMod = step(0,abs(round(IN.worldPos.z))%2-1);
			//fixed checker = abs((xMod+zMod)-1);
			//fixed4 c = lerp(_Color,_ColorB,checker);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
