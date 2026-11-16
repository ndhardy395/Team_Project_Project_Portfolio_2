using UnityEngine;

// Hides this object in browser builds, e.g. the Quit button (browsers can't be quit from inside the game).
public class HideOnWebGL : MonoBehaviour
{
    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}