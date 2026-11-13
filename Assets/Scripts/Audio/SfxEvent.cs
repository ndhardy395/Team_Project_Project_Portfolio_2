using UnityEngine;

// One sound, like "Gun Shoot" or "Enemy Death": a few takes of it plus how to play them.
// Make one with right-click in the Project window > Create > Audio > Sfx Event, drag your clips in,
// then drag the asset onto a script's sound slot (Gun's Shoot Sound, an enemy's Death Sound, ...).
[CreateAssetMenu(menuName = "Audio/Sfx Event", fileName = "New Sfx")]
public class SfxEvent : ScriptableObject
{
    public AudioClip[] clips;                                // one is picked at random each play, so repeats don't sound copy-pasted
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 0.5f)] public float pitchVariation = 0.08f;  // each play is randomly this much higher or lower
    [Range(0f, 1f)] public float spatialBlend = 0f;          // 0 = same loudness anywhere on screen (best for our overhead camera), 1 = fully 3D
    [Tooltip("A play this soon after the last one is skipped, so ten towers firing at once don't pile up into noise.")]
    public float minInterval = 0.05f;

    [System.NonSerialized] float lastPlayed = -999f;

    public void Play(Vector3 position)
    {
        if (clips == null || clips.Length == 0) return;

        float now = Time.unscaledTime;
        if (now >= lastPlayed && now - lastPlayed < minInterval) return;   // the >= check covers a value left over from an earlier Play session
        lastPlayed = now;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;
        Sfx.PlayClip(clip, position, volume, 1f + Random.Range(-pitchVariation, pitchVariation), spatialBlend);
    }
}
