Shader "Custom/GraphIt2"
{
	Properties
	{
        _Color ("Main Color", Color) = (1,1,1,1)
        _BottomLineHeight ("BottomLine", Float) = 0.0
        _LineHeight ("LineHeight", Float) = 0.0
        _DataHeightMaxLimit ("DataHeightMaxLimit", Float) = 10.0
	}
	
	SubShader
	{
        Pass
		{
			Tags { "Queue" = "Overlay" }
			Blend SrcAlpha OneMinusSrcAlpha
			Lighting Off
			ZWrite Off
			ZTest Always


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			
            #include "UnityCG.cginc"
			
			fixed4 _Color; 
            float  _BottomLineHeight;
            float  _LineHeight;
            float  _DataHeightMaxLimit;
            struct v2f {
                float4 pos : SV_POSITION;
				float4 color : COLOR;
            };

            v2f vert (appdata_full v)
            {
                v2f o;
				if(_BottomLineHeight-v.vertex.y >_LineHeight)
                {
					v.vertex.y =_BottomLineHeight-_LineHeight;
                    v.color = float4(1,0,0,1);
                }
                o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                o.color = v.color * _Color;
				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {   
                float t = (i.pos.y - _BottomLineHeight)/_LineHeight;
                return i.color=i.color*lerp(1,0,t);
			}
            ENDCG
        }
    }
}
