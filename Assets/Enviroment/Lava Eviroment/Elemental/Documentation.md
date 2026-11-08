# RekindledFX — Lava

**Procedural lava flow shader + rising embers for Unity URP**
Version 1.0.0

---

## 1. Requirements & Setup

- **Render pipeline:** Universal Render Pipeline (URP). This package is **URP only** — it does not support the Built-in Render Pipeline or HDRP in this version.
- **Unity version:** Developed and tested on Unity 6 (URP).
- **Bloom (recommended):** The lava's heat glow is HDR emissive. For the intended look, add a **Global Volume** with **Bloom** enabled. Everything still works without it, but the glow is much stronger with Bloom on.
- **No setup steps, no dependencies.** All textures are generated procedurally at runtime. Just import the package and use the prefabs or material.

---

## 2. Quick Start (60 seconds)

1. Open **`Assets/RekindledFX/Elemental/Demo/LavaDemo.unity`**.
2. Look at the Scene view — the lava previews live in edit mode. Press **Play** to see it running.
3. To use it in your own scene, either:
   - Drag the **`LavaPool`** prefab (`Assets/RekindledFX/Elemental/Prefabs/`) into your scene, **or**
   - Apply the **`Lava`** material (`Assets/RekindledFX/Elemental/Materials/`) to any mesh, then add a **`LavaEmbers`** prefab/component above it.

---

## 3. What's Included

| Item | Path | Description |
|------|------|-------------|
| **LavaFlow** shader | `Shaders/LavaFlow.shader` | The animated lava surface shader (`RekindledFX/Elemental/LavaFlow`). |
| **ElementalParticle** shader | `Shaders/ElementalParticle.shader` | Additive particle shader used by the embers. |
| **LavaEmbers** component | `Runtime/LavaEmbers.cs` | Rising ember particles, configured entirely from code. |
| **Materials** | `Materials/` | `Lava` and `LavaCurrent` — ready-to-use presets of the LavaFlow shader. |
| **Prefabs** | `Prefabs/` | `LavaPool` (surface + embers) and `LavaEmbers`. |
| **Demo scene** | `Demo/LavaDemo.unity` | A minimal scene showing the effect. |

---

## 4. Component Reference — `LavaEmbers`

Add this component to a GameObject placed above your lava surface (its local XZ plane sits on the surface); embers rise upward. It builds its ParticleSystem entirely from code and previews in edit mode.

| Field | Description |
|-------|-------------|
| **Area** | Rectangular area (X, Z in meters) that embers spawn from. |
| **Rate** | Embers produced per second. |
| **Size** | Min/max particle size. |
| **Lifetime** | Min/max particle lifetime (seconds). |
| **Rise Speed** | Upward drift speed. |
| **Floatiness** | Amount of organic wobble (Noise). |
| **Start Color** | Ember color at birth (HDR — hot orange by default). |
| **End Color** | Color it cools to at end of life (HDR). |
| **Particle Shader** | Reference to the ElementalParticle shader. Assigned in the prefab so the shader is included in player builds. Leave assigned — see the note in section 7. |

---

## 5. Shader Reference — `RekindledFX/Elemental/LavaFlow`

Apply this shader (or the included `Lava` material) to any mesh. It is fully procedural and animated.

**Lava Colors**
- `Lava Hot Color`, `Lava Mid Color`, `Lava Cold Color` — the three-tone gradient of the molten surface.
- `Crust Color` — the color of the cooled crust between flows.

**Flow**
- `Flow Speed` — how fast the lava scrolls.
- `Flow Direction (XY)` — flow direction vector.
- `Flow Scale` — size of the flow pattern.
- `Turbulence` — swirl/distortion in the flow.

**Bubbles**
- `Bubble Scale`, `Bubble Speed`, `Bubble Intensity` — molten surface bubbling.

**Heat Glow (Emission)**
- `Emission Intensity` — overall glow strength (HDR; needs Bloom to shine).
- `Glow Pulse Speed`, `Glow Pulse Amount` — pulsing of the glow.
- `Emission Tint (glow color)` — tints the glow (HDR). Great for recolors.
- `Hot White Boost` — controls how much the hottest areas blow out toward white (lower = more color-preserving).

**Surface**
- `Roughness`, `Bump Strength` — surface lighting response.
- `Crust Threshold` — where crust forms vs. molten lava.

**Vertex Displacement**
- `Displacement Amplitude`, `Displacement Speed` — subtle surface movement (bulging).

**Crust Plates and Cracks** (optional — start with `Crust Plate Amount = 0`)
- `Crust Plate Amount (0=off)`, `Crust Plate Scale`, `Crust Plate Threshold` — hardened plate look.
- `Crack Width`, `Crust Flow Speed Mul`, `Crack Glow` — glowing cracks between plates.

**Edge Heat Glow** (optional)
- `Edge Glow (0=off)`, `Edge Glow Power` — fresnel rim heat glow on silhouettes.

**Bubble Pop** (optional)
- `Bubble Pop (0=off)` — occasional bursting bubbles.

**Mapping**
- `World-space Flow (seamless tiling)` — enable for large surfaces to avoid UV stretching/seams.
- `World Scale` — texel scale when World-space Flow is on.

---

## 6. Recipes

**Lava on any surface**
Create a material using `RekindledFX/Elemental/LavaFlow` (or duplicate the `Lava` material), then assign it to your mesh's Mesh Renderer.

**Large lava lakes without stretching**
Enable **World-space Flow** on the material and tune **World Scale**. This keeps the pattern seamless across big planes and terrains.

**Add rising embers**
Add the `LavaEmbers` prefab (or component) above the surface. Match its **Area** to the size of your lava, and tint **Start/End Color** to the mood you want.

**Recolor to toxic / alien lava**
Change `Lava Hot/Mid/Cold Color`, `Crust Color`, and `Emission Tint`. For a toxic green look, shift all colors toward green and set a green emission tint; lower `Hot White Boost` to keep the color saturated.

**Calmer / more violent lava**
Tune `Flow Speed`, `Turbulence`, `Bubble Intensity`, and `Bubble Pop`. Turn on `Crust Plate Amount` and `Crack Glow` for a crusted, cracked-earth look.

---

## 7. Troubleshooting / FAQ

**The lava material is pink / magenta.**
Your project is not using URP, or URP is not active. This package is URP only. Switch your project to the Universal Render Pipeline.

**The lava doesn't glow.**
Add a Global Volume with **Bloom** enabled, and/or raise `Emission Intensity` on the material. The glow is HDR emissive and relies on Bloom for the bright look.

**Embers appear pink or missing in a built player (but work in the Editor).**
Make sure the **Particle Shader** field on the `LavaEmbers` component is assigned to the ElementalParticle shader (it is pre-assigned in the included prefabs). This guarantees the shader is included in your build. If you build from a hand-made GameObject, assign it there too.

**The pattern stretches on a large plane.**
Enable **World-space Flow** on the material and adjust **World Scale**.

**Embers don't move in edit mode.**
They preview via live simulation in the Scene view. If you ever see them paused entering Play mode, they resume automatically.

---

## 8. Support & More from RekindledFX

**Support:** be.snake@gmail.com — questions, bug reports, and feature requests are welcome.

**Explore the rest of the RekindledFX procedural VFX line:**
- **RekindledFX Lightning** — procedural electric arcs
- **RekindledFX Frost & Ice** — freeze, frost coat & shatter
- **RekindledFX Volumetric Light Beam** — god rays & volumetric beams

All built the same way: procedural, zero dependencies, URP.

---

© RekindledFX. All rights reserved.
