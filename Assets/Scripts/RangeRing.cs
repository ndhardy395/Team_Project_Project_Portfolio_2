using UnityEngine;

// The circle the Tower Manu shows around a clicked tower. Gizmos only draw in the Scene view,
// so in-game it's a looped LineRenderer. Shared by Gun and MineTower.
public static class RangeRing
{
    static Material material;   // one material shared by every tower's ring

    public static LineRenderer Build(Transform tower, Color color, float width)
    {
        if (material == null)
            material = new Material(Shader.Find("Sprites/Default"));   // unlit, tinted by the line color

        GameObject ring = new GameObject("RangeRing");
        ring.transform.SetParent(tower, false);   // destroyed along with the tower, e.g. on upgrade

        LineRenderer line = ring.AddComponent<LineRenderer>();
        line.useWorldSpace = true;   // range is in world units, so draw the ring in world units too
        line.loop = true;
        line.positionCount = 64;
        line.widthMultiplier = width;
        line.sharedMaterial = material;
        line.startColor = color;
        line.endColor = color;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        return line;
    }

    // Call this every frame the ring is showing, so a range upgrade resizes it right away.
    public static void Draw(LineRenderer ring, Vector3 center, float radius)
    {
        float step = 2f * Mathf.PI / ring.positionCount;
        for (int i = 0; i < ring.positionCount; ++i)
        {
            Vector3 edge = new Vector3(Mathf.Cos(i * step), 0f, Mathf.Sin(i * step)) * radius;
            ring.SetPosition(i, center + edge + Vector3.up * 0.1f);   // lift it off the ground so it doesn't flicker
        }
    }
}
