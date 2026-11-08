Shader "RekindledFX/Elemental/LavaFlow"
{
    Properties
    {
        // --- Lava Colors ---
        _LavaHotColor       ("Lava Hot Color",       Color)  = (1.0,  0.85, 0.1,  1)
        _LavaMidColor       ("Lava Mid Color",        Color)  = (1.0,  0.3,  0.0,  1)
        _LavaColdColor      ("Lava Cold Color",       Color)  = (0.12, 0.02, 0.0,  1)
        _CrustColor         ("Crust Color",           Color)  = (0.08, 0.06, 0.05, 1)

        // --- Flow ---
        _FlowSpeed          ("Flow Speed",            Range(0,2))   = 0.4
        _FlowDirection      ("Flow Direction (XY)",   Vector)       = (0.3, 1.0, 0, 0)
        _FlowScale          ("Flow Scale",            Range(0.5,8)) = 2.5
        _Turbulence         ("Turbulence",            Range(0,2))   = 0.8

        // --- Bubbles ---
        _BubbleScale        ("Bubble Scale",          Range(1,20))  = 7.0
        _BubbleSpeed        ("Bubble Speed",          Range(0,3))   = 0.6
        _BubbleIntensity    ("Bubble Intensity",      Range(0,1))   = 0.4

        // --- Heat Glow (Emission) ---
        _EmissionIntensity  ("Emission Intensity",    Range(0,15))  = 6.0
        _GlowPulseSpeed     ("Glow Pulse Speed",      Range(0,5))   = 1.2
        _GlowPulseAmount    ("Glow Pulse Amount",     Range(0,1))   = 0.25
        [HDR] _EmissionTint ("Emission Tint (glow color)", Color)   = (1,1,1,1)
        _HotWhiteBoost      ("Hot White Boost",       Range(0,1))   = 0.25

        // --- Surface ---
        _Roughness          ("Roughness",             Range(0,1))   = 0.7
        _BumpStrength       ("Bump Strength",         Range(0,3))   = 1.2
        _CrustThreshold     ("Crust Threshold",       Range(0,1))   = 0.45

        // --- Vertex Displacement ---
        _DisplacementAmp    ("Displacement Amplitude",Range(0,0.3)) = 0.06
        _DisplacementSpeed  ("Displacement Speed",    Range(0,3))   = 0.5

        // --- Crust Plates + Glowing Cracks (optional) ---
        [Header(Crust Plates and Cracks)]
        _CrustPlateAmount    ("Crust Plate Amount (0=off)", Range(0,1)) = 0.0
        _CrustScale          ("Crust Plate Scale",    Range(0.1,5))   = 1.2
        _CrustPlateThreshold ("Crust Plate Threshold",Range(0,1))     = 0.5
        _CrustCrackWidth     ("Crack Width",          Range(0.01,0.4))= 0.12
        _CrustFlowMul        ("Crust Flow Speed Mul", Range(0,1))     = 0.3
        _CrackGlow           ("Crack Glow",           Range(0,3))     = 1.2

        // --- Edge Heat Glow (fresnel rim, optional) ---
        [Header(Edge Heat Glow)]
        _EdgeGlow            ("Edge Glow (0=off)",    Range(0,3))     = 0.0
        _EdgeGlowPower       ("Edge Glow Power",      Range(0.5,8))   = 3.0

        // --- Bubble Pop (optional) ---
        [Header(Bubble Pop)]
        _BubblePop           ("Bubble Pop (0=off)",   Range(0,3))     = 0.0

        // --- Mapping ---
        [Header(Mapping)]
        [Toggle(_WORLDSPACE_FLOW)] _WorldSpaceFlow ("World-space Flow (seamless tiling)", Float) = 0
        _WorldScale          ("World Scale",          Range(0.05,2))  = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            #pragma shader_feature_local _WORLDSPACE_FLOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // -------------------------------------------------------
            // Properties CBUFFER
            // -------------------------------------------------------
            CBUFFER_START(UnityPerMaterial)
                half4 _LavaHotColor;
                half4 _LavaMidColor;
                half4 _LavaColdColor;
                half4 _CrustColor;
                half  _FlowSpeed;
                float4 _FlowDirection;
                half  _FlowScale;
                half  _Turbulence;
                half  _BubbleScale;
                half  _BubbleSpeed;
                half  _BubbleIntensity;
                half  _EmissionIntensity;
                half  _GlowPulseSpeed;
                half  _GlowPulseAmount;
                half4 _EmissionTint;
                half  _HotWhiteBoost;
                half  _Roughness;
                half  _BumpStrength;
                half  _CrustThreshold;
                half  _DisplacementAmp;
                half  _DisplacementSpeed;
                half  _CrustPlateAmount;
                half  _CrustScale;
                half  _CrustPlateThreshold;
                half  _CrustCrackWidth;
                half  _CrustFlowMul;
                half  _CrackGlow;
                half  _EdgeGlow;
                half  _EdgeGlowPower;
                half  _BubblePop;
                half  _WorldScale;
            CBUFFER_END

            // -------------------------------------------------------
            // Structs
            // -------------------------------------------------------
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 viewDirWS  : TEXCOORD2;
                float2 uv         : TEXCOORD3;
                float3 positionOS : TEXCOORD4;
                float  fogFactor  : TEXCOORD5;
            };

            // -------------------------------------------------------
            // Noise Functions
            // -------------------------------------------------------

            // Hash function
            float3 hash3(float3 p)
            {
                p = frac(p * float3(443.8975, 397.2973, 491.1871));
                p += dot(p, p.yxz + 19.19);
                return frac((p.xxy + p.yxx) * p.zyx);
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // Smooth Value Noise 2D
            float noise2D(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                fp = fp * fp * (3.0 - 2.0 * fp);

                float a = hash(ip);
                float b = hash(ip + float2(1,0));
                float c = hash(ip + float2(0,1));
                float d = hash(ip + float2(1,1));

                return lerp(lerp(a,b,fp.x), lerp(c,d,fp.x), fp.y);
            }

            // FBM 2D — for flow
            float fbm2D(float2 p, int octaves)
            {
                float val = 0.0;
                float amp = 0.5;
                float freq = 1.0;
                for (int i = 0; i < octaves; i++)
                {
                    val += amp * noise2D(p * freq);
                    freq *= 2.1;
                    amp  *= 0.5;
                }
                return val;
            }

            // Smooth Value Noise 3D
            float noise3D(float3 p)
            {
                float3 ip = floor(p);
                float3 fp = frac(p);
                fp = fp * fp * (3.0 - 2.0 * fp);

                float n = dot(ip, float3(1.0, 157.0, 113.0));
                float4 v;
                v.x = frac(sin(n)             * 43758.5453);
                v.y = frac(sin(n + 1.0)       * 43758.5453);
                v.z = frac(sin(n + 157.0)     * 43758.5453);
                v.w = frac(sin(n + 158.0)     * 43758.5453);

                return lerp(lerp(v.x, v.y, fp.x),
                            lerp(v.z, v.w, fp.x), fp.y);
            }

            // FBM 3D
            float fbm3D(float3 p, int octaves)
            {
                float val = 0.0;
                float amp = 0.5;
                for (int i = 0; i < octaves; i++)
                {
                    val += amp * noise3D(p);
                    p   *= 2.07;
                    amp *= 0.5;
                }
                return val;
            }

            // Voronoi 2D — for bubbles
            float2 voronoi2D(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float minDist = 8.0;
                float secondDist = 8.0;

                for (int y = -2; y <= 2; y++)
                for (int x = -2; x <= 2; x++)
                {
                    float2 offset = float2(x, y);
                    float2 h = hash(ip + offset);
                    // Bubbles that move over time
                    h = 0.5 + 0.5 * sin(_Time.y * _BubbleSpeed + 6.28 * h);
                    float2 r = offset - fp + h;
                    float d = dot(r, r);
                    if (d < minDist) { secondDist = minDist; minDist = d; }
                    else if (d < secondDist) { secondDist = d; }
                }
                return float2(sqrt(minDist), sqrt(secondDist));
            }

            // -------------------------------------------------------
            // Flow function — distorts UV over time
            // -------------------------------------------------------
            float2 flowUV(float2 uv, float time)
            {
                float2 dir = normalize(_FlowDirection.xy);

                // Main flow
                float2 flowedUV = uv + dir * time * _FlowSpeed;

                // Turbulence — swirls that break up the flow
                float turbX = fbm2D(uv * 2.0 + float2(time * 0.3, 0), 3) * 2.0 - 1.0;
                float turbY = fbm2D(uv * 2.0 + float2(0, time * 0.3), 3) * 2.0 - 1.0;
                flowedUV += float2(turbX, turbY) * _Turbulence * 0.15;

                return flowedUV;
            }

            // -------------------------------------------------------
            // Vertex Shader
            // -------------------------------------------------------
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Vertex displacement — lava undulation
                float time = _Time.y * _DisplacementSpeed;
                float2 dispUV = IN.uv * _FlowScale;
                float2 fUV = flowUV(dispUV, time);

                float disp = fbm2D(fUV * 1.5, 3);
                disp = disp * 2.0 - 1.0; // normalize to -1..1

                float3 displaced = IN.positionOS.xyz + IN.normalOS * disp * _DisplacementAmp;

                OUT.positionCS = TransformObjectToHClip(displaced);
                OUT.positionWS = TransformObjectToWorld(displaced);
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS  = normalize(GetWorldSpaceViewDir(OUT.positionWS));
                OUT.uv         = IN.uv;
                OUT.positionOS = IN.positionOS.xyz;
                OUT.fogFactor  = ComputeFogFactor(OUT.positionCS.z);

                return OUT;
            }

            // -------------------------------------------------------
            // Fragment Shader
            // -------------------------------------------------------
            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y;
            #ifdef _WORLDSPACE_FLOW
                // Dominant-axis triplanar projection -> seamless on floor/wall/ceiling/terrain,
                // no UVs needed. On walls the vertical (Y) axis becomes V -> downward flow possible.
                float3 anrm = abs(normalize(IN.normalWS));
                float2 baseUV;
                if (anrm.y >= anrm.x && anrm.y >= anrm.z) baseUV = IN.positionWS.xz; // horizontal surface (floor/ceiling)
                else if (anrm.x >= anrm.z)                baseUV = IN.positionWS.zy; // X-facing wall
                else                                      baseUV = IN.positionWS.xy; // Z-facing wall
                baseUV *= _WorldScale;
            #else
                float2 baseUV = IN.uv;
            #endif
                float2 uv  = baseUV * _FlowScale;

                // -----------------------------------------------
                // 1. Flow UVs — two layers at different speeds
                // -----------------------------------------------
                float2 uv1 = flowUV(uv,        time);
                float2 uv2 = flowUV(uv * 0.7,  time * 0.6 + 3.7); // different phase via offset

                // -----------------------------------------------
                // 2. Main lava noise — average of two layers
                // -----------------------------------------------
                float lava1 = fbm2D(uv1, 5);
                float lava2 = fbm2D(uv2, 4);
                float lavaMask = lava1 * 0.6 + lava2 * 0.4;

                // -----------------------------------------------
                // 3. Bubbles (Voronoi)
                // -----------------------------------------------
                float2 bubbleUV = uv * _BubbleScale / _FlowScale +
                                  normalize(_FlowDirection.xy) * time * _BubbleSpeed * 0.5;
                float2 vor = voronoi2D(bubbleUV);
                float bubbles = 1.0 - smoothstep(0.0, 0.3, vor.x); // Bubble center
                float bubbleRing = smoothstep(0.15, 0.35, vor.x) *
                                   (1.0 - smoothstep(0.35, 0.55, vor.x)); // Bubble ring

                // Add bubbles to the main lava
                lavaMask = saturate(lavaMask + bubbles * _BubbleIntensity * 0.3);

                // -----------------------------------------------
                // 3b. Crust Plates + Glowing Cracks (optional)
                // -----------------------------------------------
                float crackGlow = 0.0;
                if (_CrustPlateAmount > 0.001)
                {
                    // Large-scale plate noise flowing differently (slower) than the main lava
                    float2 cuv = uv * _CrustScale +
                                 normalize(_FlowDirection.xy) * time * _FlowSpeed * _CrustFlowMul;
                    float cn = fbm2D(cuv, 4);
                    float w  = max(_CrustCrackWidth, 0.001);
                    // 0 = solid crust plate, 1 = molten (inside a crack)
                    float plateHeat = smoothstep(_CrustPlateThreshold - w, _CrustPlateThreshold + w, cn);
                    // bright crack band around the threshold
                    crackGlow = saturate(1.0 - abs(cn - _CrustPlateThreshold) / w) * _CrackGlow * _CrustPlateAmount;
                    // blend with amount -> lavaMask drops on solid crust (goes to crust color)
                    plateHeat = lerp(1.0, plateHeat, _CrustPlateAmount);
                    lavaMask *= plateHeat;
                }

                // -----------------------------------------------
                // 4. Color Gradient
                // -----------------------------------------------
                // Very hot (bright yellow-white) -> mid (orange-red) -> cold (dark red) -> crust (black)
                half3 lavaColor;
                if (lavaMask > 0.75)
                {
                    // Very hot region
                    float t = (lavaMask - 0.75) / 0.25;
                    lavaColor = lerp(_LavaHotColor.rgb, half3(1.0, 1.0, 0.9), t * _HotWhiteBoost);
                }
                else if (lavaMask > _CrustThreshold)
                {
                    // Hot -> mid
                    float t = (lavaMask - _CrustThreshold) / (0.75 - _CrustThreshold);
                    lavaColor = lerp(_LavaMidColor.rgb, _LavaHotColor.rgb, t);
                }
                else if (lavaMask > _CrustThreshold * 0.5)
                {
                    // Mid -> cold
                    float t = (lavaMask - _CrustThreshold * 0.5) / (_CrustThreshold * 0.5);
                    lavaColor = lerp(_LavaColdColor.rgb, _LavaMidColor.rgb, t);
                }
                else
                {
                    // Crust
                    float t = lavaMask / (_CrustThreshold * 0.5);
                    lavaColor = lerp(_CrustColor.rgb, _LavaColdColor.rgb, t);
                }

                // Bubble ring — brighter edge
                lavaColor += bubbleRing * _LavaHotColor.rgb * 0.4;

                // -----------------------------------------------
                // 5. Normal (Bump)
                // -----------------------------------------------
                float3 N  = normalize(IN.normalWS);
                float3 V  = normalize(IN.viewDirWS);

                float eps = 0.02;
                float2 uv1X = flowUV(uv + float2(eps, 0), time);
                float2 uv1Y = flowUV(uv + float2(0, eps), time);
                float bX = fbm2D(uv1X, 4) - fbm2D(uv1  , 4);
                float bY = fbm2D(uv1Y, 4) - fbm2D(uv1  , 4);
                float3 bumpNormal = normalize(float3(-bX, -bY, 1.0 / _BumpStrength));

                // Tangent space -> world space (simple approximation)
                // If N is parallel to world-up (flat ground), cross = 0 -> normalize is NaN;
                // that NaN spreads across the whole screen via bloom. Pick a safe axis when parallel.
                float3 refUp     = abs(N.y) > 0.99 ? float3(0, 0, 1) : float3(0, 1, 0);
                float3 tangentWS = normalize(cross(refUp, N));
                float3 bitangWS  = cross(N, tangentWS);
                N = normalize(N + bumpNormal.x * tangentWS * _BumpStrength +
                                  bumpNormal.y * bitangWS  * _BumpStrength);

                // -----------------------------------------------
                // 6. Emission — Heat glow
                // -----------------------------------------------
                float glowPulse  = 1.0 + sin(time * _GlowPulseSpeed) * _GlowPulseAmount;
                float heatMask   = smoothstep(_CrustThreshold, 1.0, lavaMask);
                half3 emission   = lavaColor * heatMask * _EmissionIntensity * glowPulse;

                // Bubble centers glow extra (+ optional pop flash)
                float bubblePop = 1.0 + _BubblePop * pow(saturate(sin(time * _BubbleSpeed * 3.0 + vor.y * 12.0)), 8.0);
                emission += _LavaHotColor.rgb * bubbles * _EmissionIntensity * 0.5 * glowPulse * bubblePop;

                // Glowing crack emission (crust plates)
                emission += _LavaHotColor.rgb * crackGlow * _EmissionIntensity;

                // Edge heat glow (fresnel rim, optional)
                if (_EdgeGlow > 0.001)
                {
                    float rimGlow = pow(1.0 - saturate(dot(N, V)), _EdgeGlowPower);
                    emission += _LavaHotColor.rgb * rimGlow * _EdgeGlow * _EmissionIntensity;
                }

                // -----------------------------------------------
                // 7. Lighting (Diffuse + Specular)
                // -----------------------------------------------
                Light mainLight  = GetMainLight();
                float3 L         = normalize(mainLight.direction);
                float3 H         = normalize(L + V);
                float  NdotL     = saturate(dot(N, L));
                float  NdotH     = saturate(dot(N, H));

                // Crust is more matte, lava is semi-glossy
                float surfaceRoughness = lerp(0.9, _Roughness, heatMask);
                float alpha      = surfaceRoughness * surfaceRoughness;
                float specD      = alpha / max(PI * pow(NdotH * NdotH * (alpha - 1.0) + 1.0, 2.0), 0.001);
                float fresnel    = pow(1.0 - saturate(dot(N, V)), 3.0) * heatMask;

                half3 diffuse    = lavaColor * mainLight.color * NdotL * mainLight.distanceAttenuation;
                half3 specular   = mainLight.color * specD * fresnel * mainLight.distanceAttenuation * 0.3;
                half3 ambient    = SampleSH(N) * lavaColor * 0.1;

                // Additional lights
                #ifdef _ADDITIONAL_LIGHTS
                uint lightCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < lightCount; i++)
                {
                    Light light  = GetAdditionalLight(i, IN.positionWS);
                    float NdotLi = saturate(dot(N, normalize(light.direction)));
                    diffuse     += lavaColor * light.color * NdotLi * light.distanceAttenuation;
                }
                #endif

                // -----------------------------------------------
                // 8. Final
                // -----------------------------------------------
                emission *= _EmissionTint.rgb;   // steer the glow color (e.g. more red)
                half3 finalColor = diffuse + specular + ambient + emission;
                finalColor = MixFog(finalColor, IN.fogFactor);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // Shadow Caster
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex   shadowVert
            #pragma fragment shadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half  _FlowSpeed;
                float4 _FlowDirection;
                half  _FlowScale;
                half  _Turbulence;
                half  _DisplacementAmp;
                half  _DisplacementSpeed;
                half4 _LavaHotColor;
                half4 _LavaMidColor;
                half4 _LavaColdColor;
                half4 _CrustColor;
                half  _BubbleScale;
                half  _BubbleSpeed;
                half  _BubbleIntensity;
                half  _EmissionIntensity;
                half  _GlowPulseSpeed;
                half  _GlowPulseAmount;
                half  _Roughness;
                half  _BumpStrength;
                half  _CrustThreshold;
            CBUFFER_END

            float hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float noise2D(float2 p)
            {
                float2 ip = floor(p); float2 fp = frac(p);
                fp = fp*fp*(3.0-2.0*fp);
                return lerp(lerp(hash(ip),hash(ip+float2(1,0)),fp.x),
                            lerp(hash(ip+float2(0,1)),hash(ip+float2(1,1)),fp.x),fp.y);
            }
            float fbm2D(float2 p)
            {
                float v=0; float a=0.5;
                for(int i=0;i<3;i++){v+=a*noise2D(p);p*=2.1;a*=0.5;}
                return v;
            }

            struct ShadowAttr { float4 pos : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct ShadowVary { float4 pos : SV_POSITION; };

            ShadowVary shadowVert(ShadowAttr IN)
            {
                ShadowVary OUT;
                float time = _Time.y * _DisplacementSpeed;
                float2 uv  = IN.uv * _FlowScale;
                float2 dir = normalize(_FlowDirection.xy);
                float2 fUV = uv + dir * time * _FlowSpeed;
                float  disp = fbm2D(fUV * 1.5) * 2.0 - 1.0;
                float3 displaced = IN.pos.xyz + IN.normal * disp * _DisplacementAmp;
                OUT.pos = TransformObjectToHClip(displaced);
                return OUT;
            }
            half4 shadowFrag(ShadowVary IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
