using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Plays every sound effect from a small pool of AudioSources.
// Nothing to put in a scene: it makes itself when the game starts and lives across scene loads.
// From any script: Sfx.Play(someSfxEvent, position);
public class Sfx : MonoBehaviour
{
    const int PoolSize = 24;                          // most sounds at once; past that the oldest one is cut off
    const string ButtonClickPath = "Audio/UI Click";  // Assets/Resources/Audio/UI Click.asset plays on every UI Button

    public static float Volume = 1f;   // master SFX volume, 0 to 1, for a Settings slider

    static Sfx instance;
    AudioSource[] pool;
    int next;
    SfxEvent buttonClick;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        GameObject go = new GameObject("Sfx");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<Sfx>();
    }

    void Awake()
    {
        pool = new AudioSource[PoolSize];
        for (int i = 0; i < PoolSize; ++i)
        {
            GameObject voice = new GameObject("Voice " + i);
            voice.transform.SetParent(transform);
            pool[i] = voice.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
        }

        buttonClick = Resources.Load<SfxEvent>(ButtonClickPath);
        SceneManager.sceneLoaded += HookButtons;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= HookButtons;
    }

    // Safe with an empty slot: a script with no sound assigned just stays quiet.
    public static void Play(SfxEvent sfx, Vector3 position)
    {
        if (sfx != null) sfx.Play(position);
    }

    public static void PlayClip(AudioClip clip, Vector3 position, float volume, float pitch, float spatialBlend)
    {
        if (instance == null) return;

        AudioSource source = instance.NextVoice();
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume * Volume;
        source.pitch = pitch;
        source.spatialBlend = spatialBlend;
        source.Play();
    }

    // A source that isn't playing; if all are busy, the one started longest ago.
    AudioSource NextVoice()
    {
        for (int i = 0; i < pool.Length; ++i)
        {
            int index = (next + i) % pool.Length;
            if (!pool[index].isPlaying)
            {
                next = (index + 1) % pool.Length;
                return pool[index];
            }
        }

        AudioSource oldest = pool[next];
        next = (next + 1) % pool.Length;
        return oldest;
    }

    // Every Button in each scene that loads, hidden menus included, plays the click sound.
    void HookButtons(Scene scene, LoadSceneMode mode)
    {
        if (buttonClick == null) return;

        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include))
        {
            button.onClick.RemoveListener(PlayButtonClick);   // ClickCode's menus survive scene loads; don't hook them twice
            button.onClick.AddListener(PlayButtonClick);
        }
    }

    static void PlayButtonClick()
    {
        if (instance != null) Play(instance.buttonClick, Vector3.zero);
    }
}
