using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// The popup that opens when a level on the map is clicked: best wave, best clear,
// the four temperaments to pick from, and START.
public class LevelDetailsPopup : MonoBehaviour
{
    // One of the four temperament buttons.
    [System.Serializable]
    public class TemperamentChoice
    {
        public Temperament temperament;
        public Button button;
        public Image border;      // the thin edge, recolored when picked
        public Image highlight;   // a thicker ring just outside the edge, only shown when picked
    }

    [Header("Header")]
    public TMP_Text levelLabel;
    public TMP_Text title;
    public Button closeButton;
    public Button backdrop;       // clicking the dark area outside the dialog closes it too

    [Header("Left column")]
    public Image preview;
    public GameObject noPreview;
    public TMP_Text bestWave;
    public TMP_Text bestClearLabel;
    public Image[] bestStars;

    [Header("Temperament")]
    public TemperamentChoice[] choices;
    public TMP_Text earnText;
    public Image[] earnStars;

    [Header("Start")]
    public Button startButton;
    public TMP_Text startLabel;
    public GameObject startIcon;

    [Header("Sprites")]
    public Sprite starSprite;
    public Sprite starOutlineSprite;

    LevelNode node;
    Temperament picked = Temperament.Annoyance;

    public bool IsOpen => gameObject.activeSelf;

    void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        backdrop.onClick.AddListener(Hide);
        startButton.onClick.AddListener(StartLevel);
        foreach (TemperamentChoice choice in choices)
        {
            Temperament temperament = choice.temperament;
            choice.button.onClick.AddListener(() => Pick(temperament));
        }
    }

    public void Open(LevelNode levelNode)
    {
        node = levelNode;
        LevelData level = node.level;

        levelLabel.text = node.label;
        title.text = level.displayName;

        preview.sprite = level.previewImage;
        preview.gameObject.SetActive(level.previewImage != null);
        noPreview.SetActive(level.previewImage == null);

        int best = SaveManager.GetStars(level);
        int wave = SaveManager.GetBestWave(level);
        bestWave.text = wave > 0 ? wave.ToString() : "-";
        bestClearLabel.text = best > 0 ? $"BEST CLEAR · {((Temperament)best).ToString().ToUpper()}" : "BEST CLEAR · NONE YET";
        LevelSelectStyle.PaintStars(bestStars, best, starSprite, starOutlineSprite);

        // Pick up where the player left off: the temperament they last played this level on,
        // otherwise one step above their best clear, so the next star is the default goal.
        if (LevelSession.Level == level)
            picked = LevelSession.Temperament;
        else
            picked = (Temperament)Mathf.Clamp(best + 1, (int)Temperament.Annoyance, (int)Temperament.Bloodlust);

        bool playable = !string.IsNullOrEmpty(level.sceneName) && Application.CanStreamedLevelBeLoaded(level.sceneName);
        startButton.interactable = playable;
        startLabel.text = playable ? "START" : "COMING SOON";
        startIcon.SetActive(playable);

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Pick(picked);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void Pick(Temperament temperament)
    {
        picked = temperament;
        foreach (TemperamentChoice choice in choices)
        {
            bool on = choice.temperament == temperament;
            Color color = LevelSelectStyle.TemperamentColor(choice.temperament);
            choice.highlight.gameObject.SetActive(on);
            choice.highlight.color = color;
            choice.border.color = on ? color : LevelSelectStyle.LockedBorder;
        }

        int stars = (int)temperament;
        string starWord = stars == 1 ? "1 star" : $"{stars} stars";
        earnText.text = $"Clear on <color=#EEF2F6><b>{temperament}</b></color> to earn {starWord}";
        LevelSelectStyle.PaintStars(earnStars, stars, starSprite, starOutlineSprite);
    }

    void StartLevel()
    {
        LevelData level = node.level;
        LevelSession.Begin(level, picked);
        Hide();

        if (ClickCode.Instance != null)
            ClickCode.Instance.StartLevel(level.sceneName);
        else
            SceneManager.LoadScene(level.sceneName);
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Hide();
#endif
    }
}
