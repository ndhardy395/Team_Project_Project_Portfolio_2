using UnityEngine;
using UnityEngine.InputSystem; //required for new input system

public class WaveSkipper : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float earlySendCooldown = 0.75f;
    private float lastSkipTime = -999f;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (Time.time >= lastSkipTime + earlySendCooldown)
            {
                if (WaveSpawner.Instance != null)
                {
                    WaveSpawner.Instance.ForceNextWave();
                    lastSkipTime = Time.time; // Update cooldown timestamp
                    Debug.Log("Spacebar pressed: Skipped wave delay!");
                }
            }
        }
    }
}