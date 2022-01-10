Shader "Unlit/AlphaSelfIllum" {
    Properties{
           _Color("Main Color (A=Opacity)", Color) = (0,0,1,1)
           _MainTex("Base (A=Opacity)", 2D) = "1"
    }

        Category{
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True"}
            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha

            SubShader {
            Pass {

                GLSLPROGRAM
                out mediump vec2 uv;

                #ifdef VERTEX
                uniform mediump vec4 _MainTex_ST;
                void main() {
                    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
                    uv = gl_MultiTexCoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                }
                #endif

                #ifdef FRAGMENT
                uniform lowp sampler2D _MainTex;
                uniform lowp vec4 _Color;
                void main() {
                    gl_FragColor = texture2D(_MainTex, uv) * _Color;
                }
                #endif     
                ENDGLSL
              }
            }

            SubShader {
            Pass {
                SetTexture[_MainTex] {Combine texture * constant ConstantColor[_Color]}
            }
          }
    }
}
