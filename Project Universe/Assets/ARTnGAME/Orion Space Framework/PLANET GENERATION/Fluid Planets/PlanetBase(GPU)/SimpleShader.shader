﻿Shader "ORION/SimplePlanetShader" {
	Properties {
    
		_ColorA ("Color", Color) = (1,1,1,1)
        _ColorB ("Color", Color) = (1,1,1,1)
        _ColorC ("Color", Color) = (1,1,1,1)
        _ColorD ("Color", Color) = (1,1,1,1)
                                                
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Velo ("_Velo", 2D) = "black" {}
                       
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
						 brightnessContrast("_SunLight", Vector) = (1,1,1,1)
	}
    
    // Note 
    // This shader turns the densitity & velocity information into
    // an visual Texture which then is applied to the planet
    
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
        // Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
        //#pragma exclude_renderers d3d11
		
		#pragma surface surf Standard fullforwardshadows
		//#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		
        float4 _ColorA;
		float4 _ColorB;
		float4 _ColorC;
		float4 _ColorD;
		float4 _ColorD_1;
        
        sampler2D _Velo;    

		float4 brightnessContrast;
              
		void surf (Input IN, inout SurfaceOutputStandard o) {
        
		   float4 c = tex2D (_MainTex, IN.uv_MainTex);
           float4 cf = c/ (c+1);
          
		   float4 v = tex2D (_Velo, IN.uv_MainTex);
           float fakelight=pow(max(0,dot(v.xy, normalize(float2(-1,-1)))+0.22),2)*0.5+0.5;
           
           o.Albedo  =lerp(lerp(lerp(_ColorA,_ColorB, cf.r),_ColorC, cf.g), _ColorD, cf.b).rgb;           
		   o.Albedo *= (length(v.xy)*0.5 + 0.5) * 2 * fakelight;// *_SunLight;
           
		   o.Albedo = pow(o.Albedo, brightnessContrast.y) * brightnessContrast.x;

           o.Metallic = _Metallic;
		   o.Smoothness = _Glossiness;
		   o.Alpha = 1;
		
        }
		ENDCG
	}
    
	FallBack "Diffuse"
}
