// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Cutout-Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	_InSideWater("In Water", Float) = 0
	_WaterPlaneY("Water Plane Y", Float) = 0
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 200
	
CGPROGRAM
#pragma surface surf Lambert alphatest:_Cutoff vertex:vert

sampler2D _MainTex;
fixed4 _Color;
float _InSideWater;
float _WaterPlaneY;

struct Input {
	float2 uv_MainTex;
};

void vert(inout appdata_full v) {

	if (_InSideWater == 1)
	{
		float4 vertex = mul(_Object2World, v.vertex);
		if (vertex.y > _WaterPlaneY)
		{
			float distance = length(vertex);
			float sinrad = sin(3.14 * distance * _Time.y * 0.01)* 2;
			v.vertex += fixed4(0.01, 0.02, 0.01, 0) * sinrad ;
			
		}
	}
}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Transparent/Cutout/VertexLit"
}
