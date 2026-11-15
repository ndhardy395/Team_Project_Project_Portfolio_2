using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TipBar : MonoBehaviour
{
    [Header("UI Refrences")]
    [SerializeField] private GameObject tipPanel;
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private Button closeButton;

    [SerializeField] private Toggle dontShowAgianToggle;
    [SerializeField] private TMP_Text tipCounterText;


[Header("Tips")]
[SerializeField,Min(1f)] private float secondsPerTip;
    [SerializeField, TextArea(2, 4)]
    private string[] tips =
        {
        "Click an enemy to view its HP and Information.",
        "Each tower upgrades has its own perks. Check the details before choosing.",
        "Place tower where they can cover more of the enemy path.",
        "Use spells to support your towers when enemies start getting through.",
        "Calling the next wave early brings more enemies and extra rewards. Make sure your ready!"
    };


    private const string HideTipsKey = "SkyLineRush_HideTips";

    private int currentTip;

    private float timer;

    private void Start()
    {
        if(tipPanel == null || tipText == null || closeButton == null || dontShowAgianToggle == null)
        {
            Debug.LogError("Assign all TipBar UI refrence.", this);
            enabled = false;
            return;
        }

        closeButton.onClick.AddListener(CloseTips);

        // the toggle affects future games when the player closes the bar.
        dontShowAgianToggle.SetIsOnWithoutNotify(false);

        bool showTips = PlayerPrefs.GetInt(HideTipsKey, 0) == 0;
        tipPanel.SetActive(showTips);

        if(showTips&& tips != null && tips.Length > 0)
        {
            tipText.text = tips[0];
            UpdateTipCounter();
        }
    }



    private void Update()
    {
        if(tipPanel == null || !tipPanel.activeInHierarchy || tips == null || tips.Length == 0)
        {
            return;
        }

        // Keep each tip readable for the same real duuration at any game speed.
        // pause rotation when the game is paused waiting to start.

        timer += Time.unscaledDeltaTime;

        if(timer >= secondsPerTip)
        {
            timer = 0f;
            currentTip = (currentTip + 1) % tips.Length;
            tipText.text = tips[currentTip];
            UpdateTipCounter();
        }
    }

    public void CloseTips()
    {
        PlayerPrefs.SetInt(HideTipsKey, dontShowAgianToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
        tipPanel.SetActive(false);
    }

    // Can also be connected to a settings button to restor tips

    [ContextMenu("Enable Tips Again")]
    public void EnableTipsAgain()
    {
        PlayerPrefs.DeleteKey(HideTipsKey);
        PlayerPrefs.Save();

        currentTip = 0;
        timer = 0f;

        if(dontShowAgianToggle != null)
        {
            dontShowAgianToggle.SetIsOnWithoutNotify(false);
        }

        if(tipPanel != null)
        {
            tipPanel.SetActive(true);
        }

        if(tipText != null && tips != null && tips.Length > 0)
        {
            tipText.text = tips[0];
            UpdateTipCounter(); 
        }
    }

    private void UpdateTipCounter()
    {
        if(tipCounterText != null && tips != null && tips.Length > 0)
        {
            tipCounterText.text = $"Tips {currentTip + 1} / {tips.Length}";
        }
    }

    private void OnDestroy()
    {
        if(closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseTips);
        }
    }

}
