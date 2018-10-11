Shader "Sprites/SpriteInAtlas"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"RenderType" = "Transparent"
		"CanUseSpriteAtlas" = "True"
	}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing
#include "UnityCG.cginc"

		struct appdata_t
	{
		float4 vertex   : POSITION;
		float2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float4 vertex   : SV_POSITION;
		float2 texcoord  : TEXCOORD0;
		//UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	
	v2f vert(appdata_t IN)
	{
		v2f OUT;
		//UNITY_SETUP_INSTANCE_ID(IN);
		OUT.vertex = UnityObjectToClipPos(IN.vertex);
		OUT.texcoord = IN.texcoord;
		return OUT;
	}

	sampler2D _MainTex;

	fixed4 frag(v2f IN) : SV_Target
	{
		//UNITY_SETUP_INSTANCE_ID(IN);
		fixed4 c = tex2D(_MainTex, IN.texcoord);
	c.rgb *= c.a;
	return c;
	}
		ENDCG
	}
	}
}
