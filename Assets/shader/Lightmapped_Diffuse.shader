Shader "Lightmapped_Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_LightMap ("Lightmap (RGB)", 2D) = "black" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
}

SubShader {
	LOD 200
	Tags {"Queue"="AlphaTest"  "RenderType"="TransparentCutout" }
	//Lighting Off
	Alphatest Greater [_Cutoff]
CGPROGRAM
#pragma surface surf Lambert
struct Input {
  float2 uv_MainTex;
  float2 uv2_LightMap;
};
sampler2D _MainTex;
sampler2D _LightMap;
fixed4 _Color;
void surf (Input IN, inout SurfaceOutput o)
{
  o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * _Color;
  half4 lm = tex2D (_LightMap, IN.uv2_LightMap);
  o.Emission = lm.rgb*o.Albedo.rgb *2.0;
  o.Alpha = tex2D (_MainTex, IN.uv_MainTex).a * _Color.a;
}
ENDCG
}
//FallBack "Legacy Shaders/Lightmapped/VertexLit"
}
