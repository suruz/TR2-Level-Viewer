// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "WaterEffect" {
	
	Properties {
	    _Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainTex2 ("Effect (RGB)", 2D) = "white" {}
		_CenterX("X Center", Float) = 0
		_CenterY("Y Center", Float) = 0
		_CenterZ("Z Center", Float) = 0
		_WaterPlaneY("Water Plane Y", Float) = 0
		_InSideWater("In Water", Float) = 0
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
	}
	SubShader {
		Tags {"Queue"="Geometry-1000" "IgnoreProjector"="True" "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf SimpleLambert vertex:vert alphatest:_Cutoff

		sampler2D _MainTex;
		sampler2D _MainTex2;
		float4 _Color;
		float _CenterX;
		float _CenterY;
		float _CenterZ;
		float _WaterPlaneY;
		float _InSideWater;

		struct Input {
			float2 uv_MainTex;
			float2 uv2_MainTex2;
			//float3 customColor;
		};
		
		 half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
          half NdotL = pow(dot(s.Normal, normalize(fixed3(1,0.5,1))), 4) * 1.15;
          half4 c;
          c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten );
          c.a = s.Alpha;
          return c;
      }
		
	  void vert (inout appdata_full v) {
	  
	       //float3 _vertex = normalize(v.vertex - float3(_CenterX , _CenterY, _CenterZ + 0.5));
	       //float _v =  dot(float3(0,0,1), normalize(float3(0,_vertex.y,_vertex.z))) * 0.5 + 0.5f;;
	       //float _u =  dot(float3(1,0,0), normalize(float3(_vertex.x,0,_vertex.z))) * 0.5 + 0.5f;//      _vertex.x * 0.5 + 0.5;
           //v.texcoord1.xy   = float2(_u, _v) ;
           //o.customColor = _vertex * 0.5 + float3(0.5,0.5,0.5);

		  float4 vertex = mul(_Object2World, v.vertex);
		  float distance = length(vertex.xyz - half3(_CenterX, _CenterY, _CenterZ));
		  float rad = 3.14 * distance * _Time.y;
		  v.normal += float3(sin(rad  * 0.25) + cos(rad  * 0.25),  0 , sin(rad  * 0.25) + cos(rad  * 0.25) ) ;

		   if (_InSideWater == 0)
		   {
			   if (vertex.y < _CenterY)
			   {
				   distance = length(vertex);
				   float sinrad = sin(3.14 * distance * _Time.y * 0.01) * 2;
				   v.vertex += fixed4(0.01, 0.02, 0.01, 0) * sinrad;
			   }
		   }
      }

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c =  tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb ;//* IN.customColor;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
