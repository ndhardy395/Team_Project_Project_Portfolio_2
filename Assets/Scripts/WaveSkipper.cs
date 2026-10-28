using UnityEngine;
using UnityEngine.InputSystem; // Required for the New Input System

public class WaveSkipper : MonoBehaviour
{
    void Update()
    {
        // Check if the keyboard exists and the space key was pressed this frame
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // Check if the WaveSpawner exists in the scene
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.ForceNextWave();
                Debug.Log("Spacebar pressed: Skipped wave delay!");
            }
        }
    }
}