Shader "MONA/Brains/BoundingBoxLine"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Fog { Mode Off }

            BindChannels {
                Bind "Color", color
                Bind "Vertex", vertex
            }

            SetTexture[_MainTex] {
                combine primary
            }
        }
    }
}
