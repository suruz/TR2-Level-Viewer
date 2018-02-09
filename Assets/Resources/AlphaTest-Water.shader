Shader "AlphaTest-Water" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("MainTex ", 2D) = "white" {}
    _MainTex2("MainTex2", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	_UVToggle("UV Switch", Range(0,1)) = 0.0 
	
	_ReflectionColor("Reflection Color", Color) = (0.26,0.19,0.16,0.0)
	_Metalic("Metalic", Range(0.5,128.0)) = 3.0
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200
	
CGPROGRAM
#pragma surface surf Lambert alpha 

sampler2D _MainTex;
sampler2D _MainTex2;
//samplerCUBE _Cube;

fixed4 _Color;
float _UVToggle;
float4 _ReflectionColor;
float _Metalic;

struct Input {
	float2 uv_MainTex;
	float2 uv2_MainTex2;
	float3 worldRefl;
	float3 viewDir;
};

void surf (Input IN, inout SurfaceOutput o) {

	half fresnel = saturate(dot(normalize(IN.viewDir), o.Normal));
	half fresnelpower = pow(fresnel, _Metalic);
	//fixed4 reflcol = texCUBE(_Cube, IN.worldRefl);
	
    fixed4 color = lerp(tex2D(_MainTex, IN.uv_MainTex),tex2D(_MainTex, IN.uv2_MainTex2) , _UVToggle) * 1;
    //float2 uv = lerp(IN.uv_MainTex, IN.uv2_MainTex2, _UVToggle);
	fixed4 c = color ;//* lerp(_Color, fixed4(1,1,1,1), color.a);  
	o.Albedo = c.rgb;
	o.Alpha = lerp(0.5 * fresnelpower , 1, c.a);
	
	if(c.a < 0.5)
	{
	  o.Albedo  = _Color;
	}
	
	o.Emission = _ReflectionColor.rgb * fresnelpower * _LightColor0.rgb;// * reflcol.rgb;
}
ENDCG
}

Fallback "Transparent/Cutout/VertexLit"
}
