using UnityEngine;

// A dashed trail on the 3D map between two stops.
// It lights up (gold, or blue for a branch to a key level) once the level it starts from is beaten.
public class LevelTrail : MonoBehaviour
{
    [Tooltip("The level this trail starts from. Beating it lights the trail.")]
    public LevelData from;
    [Tooltip("Branch to a key level (blue) instead of the main road (gold).")]
    public bool keyBranch;
    public Renderer[] dashes;

    public void Refresh(Material lit, Material unlit)
    {
        Material material = from != null && SaveManager.IsBeaten(from) ? lit : unlit;
        foreach (Renderer dash in dashes)
            dash.sharedMaterial = material;
    }
}
