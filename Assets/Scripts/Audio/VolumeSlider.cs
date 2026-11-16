using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Master volume for the Settings menu's audio panel. Put it on the Slider.
// It turns every sound in the game up or down at once (AudioListener.volume), so music added later is covered too.
// The value is saved, and put back when the game starts, even if the player never opens Settings.
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour, IPointerUpHandler
{
    const string VolumeKey = "SkyLineRush_MasterVolume";   // same prefix as TipBar's saved setting

    [SerializeField] TMP_Text percentText;   // optional: shows "80%" next to the slider
    [SerializeField] SfxEvent previewSound;  // optional: plays when you let go, so you hear the new level (UI Click works)

    Slider slider;

    // Runs once when the game starts, before the first scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplySavedVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
    }

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.onValueChanged.AddListener(SetVolume);
    }

    // The Settings menu starts hidden, so show the current volume each time it opens.
    void OnEnable()
    {
        slider.SetValueWithoutNotify(AudioListener.volume);
        ShowPercent(AudioListener.volume);
    }

    // Write the setting to disk once, when the panel closes, instead of on every step of a drag.
    void OnDisable()
    {
        PlayerPrefs.Save();
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        ShowPercent(value);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Sfx.Play(previewSound, Vector3.zero);
    }

    void ShowPercent(float value)
    {
        if (percentText != null) percentText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}