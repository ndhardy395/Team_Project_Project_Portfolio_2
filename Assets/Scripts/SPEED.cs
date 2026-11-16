using UnityEngine;

public class SPEED : MonoBehaviour //Awww yeah! GOTTA GO FAST!
{
    [Tooltip("Target speed multiplier for this scene (set to 2 for double speed).")]
    public float targetSpeed = 2f; //speed multiplier lol

    private float defaultFixedDeltaTime;

    void Awake()
    {
        // Save the default physics timestep
        defaultFixedDeltaTime = Time.fixedDeltaTime;

        // Apply the 2x speed globally for this scene. EVERYTHING MUST GO FAST!
        Time.timeScale = targetSpeed;

        // Scale physics fixed delta time proportionally to prevent jitter
        Time.fixedDeltaTime = defaultFixedDeltaTime * targetSpeed;
    }

    void OnDestroy()
    {
        // Crucial safety check: Resets the speed once you leave. Don't want EVERYTHING fast all the time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}
