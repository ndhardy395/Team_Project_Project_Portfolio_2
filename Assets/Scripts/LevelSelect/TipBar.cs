using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TipBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tipPanel;
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Toggle dontShowAgianToggle;
    [SerializeField] private TMP_Text tipCounterText;

    [Header("Tips")]
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
    private Button nextTipButton;
    private Button openTipsButton;

    private bool HasTips => tips != null && tips.Length > 0;

    private void Start()
    {
        if (tipPanel == null || tipText == null || closeButton == null || dontShowAgianToggle == null)
        {
            Debug.LogError("Assign all TipBar UI references.", this);
            enabled = false;
            return;
        }

        closeButton.onClick.AddListener(CloseTips);

        if (HasTips)
        {
            nextTipButton = CreateButton("Next Tip", tipPanel.transform, new Vector2(-12f, 12f),
                new Vector2(105f, 34f), AdvanceTip);
            nextTipButton.interactable = tips.Length > 1;

            // Keep the reopen button outside the panel so closing the panel leaves it accessible.
            openTipsButton = CreateButton("Tips", tipPanel.transform.parent, new Vector2(-20f, 20f),
                new Vector2(118f, 64f), OpenTips);
        }

        bool showTips = PlayerPrefs.GetInt(HideTipsKey, 0) == 0;
        dontShowAgianToggle.SetIsOnWithoutNotify(!showTips);
        SetTipsVisible(showTips && HasTips);
    }

    private void AdvanceTip()
    {
        if (!HasTips || tipPanel == null || !tipPanel.activeInHierarchy)
            return;

        currentTip = (currentTip + 1) % tips.Length;
        ShowCurrentTip();
    }

    public void CloseTips()
    {
        PlayerPrefs.SetInt(HideTipsKey, dontShowAgianToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
        SetTipsVisible(false);
    }

    private void OpenTips()
    {
        SetTipsVisible(HasTips);
    }

    [ContextMenu("Enable Tips Again")]
    public void EnableTipsAgain()
    {
        PlayerPrefs.DeleteKey(HideTipsKey);
        PlayerPrefs.Save();

        currentTip = 0;
        if (dontShowAgianToggle != null)
            dontShowAgianToggle.SetIsOnWithoutNotify(false);

        SetTipsVisible(HasTips);
    }

    private void SetTipsVisible(bool visible)
    {
        if (tipPanel != null)
            tipPanel.SetActive(visible);

        if (openTipsButton != null)
            openTipsButton.gameObject.SetActive(!visible);

        if (visible)
            ShowCurrentTip();
    }

    private void ShowCurrentTip()
    {
        if (tipText != null)
            tipText.text = tips[currentTip];

        if (tipCounterText != null)
            tipCounterText.text = $"Tips {currentTip + 1} / {tips.Length}";
    }

    private Button CreateButton(string label, Transform parent, Vector2 position, Vector2 size, UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.layer = parent.gameObject.layer;
        RectTransform rect = (RectTransform)buttonObject.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image background = buttonObject.GetComponent<Image>();
        background.color = new Color(0.16f, 0.18f, 0.21f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform labelRect = (RectTransform)labelObject.transform;
        labelRect.SetParent(rect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(4f, 2f);
        labelRect.offsetMax = new Vector2(-4f, -2f);

        TMP_Text buttonText = labelObject.GetComponent<TextMeshProUGUI>();
        buttonText.font = tipText.font;
        buttonText.text = label;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMin = 12f;
        buttonText.fontSizeMax = 32f;
        buttonText.raycastTarget = false;
        return button;
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseTips);

        if (nextTipButton != null)
            Destroy(nextTipButton.gameObject);
    }
}
