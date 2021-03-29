Shader "Custom/MySSAO"
{
	Properties
	{
	}
	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float3 viewRay : TEXCOORD1;
	};

	//顶点阶段
	#define MAX_SAMPLE_POINT_COUNT 32
	//源画面
	sampler2D _MainTex;
	//噪声画面
	sampler2D _NoiseTex;
	//深度法线画面
	sampler2D _CameraDepthNormalsTexture;
	//反投影矩阵,用于获得相机视角每一个像素所对应的顶点坐标
	float4x4 _InverseProjectionMatrix;
	//采样点数组,最多32个
	float4 _SamplePointArray[MAX_SAMPLE_POINT_COUNT];
	float _SamplePointCount;
	float _SampleKernelRadius;
	float _Bias;
	float _Strength;

	//光栅化阶段
	float4 _MainTex_TexelSize;
	float4 _BlurRadius;
	float _BilaterFilterFactor;

	//输出阶段
	sampler2D _AOTex;
	
	//顶点阶段
	v2f vert_ao(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		//获得View Space中点坐标
		float4 clipPos = float4(v.uv * 2.0 - 1.0, 1.0, 1.0);
		float4 viewRay = mul(_InverseProjectionMatrix, clipPos);
		o.viewRay = viewRay.xyz / viewRay.w;
		return o;
	}

	fixed4 frag_ao(v2f i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.uv);

		//深度值
		float pPointDepth;
		//法线值
		float3 viewNormal;

		//在深度法线画面上取样
		float4 cdn = tex2D(_CameraDepthNormalsTexture, i.uv);
		DecodeDepthNormal(cdn, pPointDepth, viewNormal);

		float3 viewPos = pPointDepth * i.viewRay;

		//铺平纹理
		float2 noiseScale = _ScreenParams.xy;
		float2 noiseUV = float2(i.uv.x * noiseScale.x, i.uv.y * noiseScale.y);
		//采样噪声贴图
		float3 randvec = tex2D(_NoiseTex, noiseUV).xyz;
		//Gramm-Schimidt方法处理创建正交基
		float3 tangent = normalize(randvec - viewNormal * dot(randvec, viewNormal));
		float3 bitangent = cross(viewNormal, tangent);
		#if UNITY_UV_STARTS_AT_TOP
		float3x3 TBN = float3x3(tangent, bitangent, viewNormal);
		#else
		float3x3 TBN = float3x3(tangent, -viewNormal, bitangent);
		#endif

		int sampleCount = _SamplePointCount;
		float oc = 0.0;
		for (int i = 0; i < sampleCount; ++i)
		{
			float3 randomVector = mul(_SamplePointArray[i].xyz, TBN);
			//float3 randomVector = mul(TBN,_SamplePointArray[i].xyz);
			//randomVector = normalize(randomVector);
			//randomVector = dot(randvec,viewNormal) < 0 ? -randvec : randvec;

			float3 randomPos = viewPos + randomVector * _SampleKernelRadius;
			float3 rclipPos = mul((float3x3)unity_CameraProjection, randomPos);
			float2 rscreenPos = (rclipPos.xy / rclipPos.z) * 0.5 + 0.5;

			float samplePointDepth;
			float3 randomNormal;
			float4 rcdn = tex2D(_CameraDepthNormalsTexture, rscreenPos);
			DecodeDepthNormal(rcdn, samplePointDepth, randomNormal);

			//确定该取样点的权重
			float rangeCheck = smoothstep(0.0, 1.0, _SampleKernelRadius / abs(viewPos.z - samplePointDepth));
			oc += (pPointDepth > samplePointDepth + _Bias ? 1.0 : 0.0) * rangeCheck;
		}
		oc = 1.0 - oc / sampleCount;
		col.rgb = pow(oc, _Strength);
		return col;
	}

	fixed4 frag_composite(v2f i) : SV_Target
	{
		float2 offsetUV = i.uv;
		#if UNITY_UV_STARTS_AT_TOP
		offsetUV.y = 1.0 - offsetUV.y;
		#endif
		fixed4 ori = tex2D(_MainTex, offsetUV);
		fixed4 ao = tex2D(_AOTex, i.uv);
		//AO只有一个通道 -> 灰度信息
		ori.rgb *= ao.r;
		return ori;
	}
	
	float3 GetNormal(float2 uv)
	{
		//获得法线贴图
		float4 cdn = tex2D(_CameraDepthNormalsTexture, uv);
		return DecodeViewNormalStereo(cdn);
	}

	half CompareNormal(float3 normal1, float3 normal2)
	{
		return smoothstep(_BilaterFilterFactor, 1.0, dot(normal1, normal2));
	}


	fixed4 frag_blur(v2f i) : SV_Target
	{
		float2 delta = _MainTex_TexelSize.xy * _BlurRadius.xy;

		float2 uv = i.uv;
		float2 uv0a = i.uv - delta;
		float2 uv0b = i.uv + delta;
		float2 uv1a = i.uv - 2.0 * delta;
		float2 uv1b = i.uv + 2.0 * delta;
		float2 uv2a = i.uv - 3.0 * delta;
		float2 uv2b = i.uv + 3.0 * delta;

		float3 normal = GetNormal(uv);
		float3 normal0a = GetNormal(uv0a);
		float3 normal0b = GetNormal(uv0b);
		float3 normal1a = GetNormal(uv1a);
		float3 normal1b = GetNormal(uv1b);
		float3 normal2a = GetNormal(uv2a);
		float3 normal2b = GetNormal(uv2b);

		fixed4 col = tex2D(_MainTex, uv);
		fixed4 col0a = tex2D(_MainTex, uv0a);
		fixed4 col0b = tex2D(_MainTex, uv0b);
		fixed4 col1a = tex2D(_MainTex, uv1a);
		fixed4 col1b = tex2D(_MainTex, uv1b);
		fixed4 col2a = tex2D(_MainTex, uv2a);
		fixed4 col2b = tex2D(_MainTex, uv2b);

		half w = 0.37004405286;
		half w0a = CompareNormal(normal, normal0a) * 0.31718061674;
		half w0b = CompareNormal(normal, normal0b) * 0.31718061674;
		half w1a = CompareNormal(normal, normal1a) * 0.19823788546;
		half w1b = CompareNormal(normal, normal1b) * 0.19823788546;
		half w2a = CompareNormal(normal, normal2a) * 0.11453744493;
		half w2b = CompareNormal(normal, normal2b) * 0.11453744493;

		half3 result;
		result = w * col.rgb;
		result += w0a * col0a.rgb;
		result += w0b * col0b.rgb;
		result += w1a * col1a.rgb;
		result += w1b * col1b.rgb;
		result += w2a * col2a.rgb;
		result += w2b * col2b.rgb;

		result /= w + w0a + w0b + w1a + w1b + w2a + w2b;
		return fixed4(result, 1.0);
	}
	ENDCG

	SubShader
	{

		Cull Off ZWrite Off ZTest Always

		//Pass 0 : Generate AO 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_ao
			#pragma fragment frag_ao
			#pragma UNITY_UV_STARTS_AT_TOP
			ENDCG
		}

		//Pass 1 : Bilateral Filter Blur
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_ao
			#pragma fragment frag_blur
			#pragma UNITY_UV_STARTS_AT_TOP
			ENDCG
		}

		//Pass 2 : Composite AO
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_ao
			#pragma fragment frag_composite
			#pragma UNITY_UV_STARTS_AT_TOP
			ENDCG
		}

	}

}