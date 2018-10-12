Shader "Sprites/SpritePart"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		//_Rect("Sub-Rectangle", Vector) = (0,0,0,0)
			//_Color("Main Color", Color) = (1,1,1,1)
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
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};


		v2f vert(appdata_t IN)
		{
			v2f OUT;
			UNITY_SETUP_INSTANCE_ID(IN);
			UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
			OUT.vertex = UnityObjectToClipPos(IN.vertex);
			OUT.texcoord = IN.texcoord;
			return OUT;
		}

		sampler2D _MainTex;
		UNITY_INSTANCING_BUFFER_START(Props)
			//UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Rect)
			UNITY_INSTANCING_BUFFER_END(Props)

		fixed4 frag(v2f IN) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(IN);
		fixed4 rect = UNITY_ACCESS_INSTANCED_PROP(Props, _Rect);
			float2 uv_projection = float2(IN.texcoord.x * rect.z + rect.x, IN.texcoord.y * rect.w + rect.y);
			fixed4 c = tex2D(_MainTex, uv_projection);// *UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
		c.rgb *= c.a;
		return c;
		}
			ENDCG
		}
		}
}