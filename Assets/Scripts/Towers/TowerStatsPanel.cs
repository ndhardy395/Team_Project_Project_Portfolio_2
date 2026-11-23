using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

// The Tower Stats window: name, level, description and current numbers of the placed tower the
// player clicked. It sits on ClickCode's main canvas ("all"), pinned to the left edge with a fixed
// size, so the Canvas Scaler grows and shrinks it with the screen instead of stretching it.
//
// TowerUpgrad (the Tower Manu) opens it in Select(). It closes itself whenever the Tower Manu
// closes, whichever way that happens (Back, Sell, clicking a base, pausing, the level ending),
// so clicking an empty base never shows it.
public class TowerStatsPanel : MonoBehaviour
{
    [Header("Texts (children of this panel)")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI statsText;

    [Tooltip("Where the value column starts in the stats list, as a share of the list's width.")]
    [SerializeField, Range(0.3f, 0.9f)] float valueColumn = 0.62f;

    [Tooltip("Seconds between re-reading the tower's stats while open (unscaled, so it works paused too).")]
    [SerializeField] float refreshInterval = 0.25f;

    ITower tower;
    GameObject menu;   // the Tower Manu that opened this window; when it closes, so does this
    float nextRefresh;

    readonly List<(string label, string value)> stats = new List<(string label, string value)>();
    readonly StringBuilder lines = new StringBuilder();

    // Unity's "destroyed counts as null" only works on Unity types, not through an interface (same as TowerUpgrad).
    static bool Exists(ITower t) => t is Object obj && obj != null;

    public void Show(ITower clicked, GameObject openedBy)
    {
        if (!Exists(clicked)) { Hide(); return; }
        tower = clicked;
        menu = openedBy;
        gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        tower = null;
        menu = null;
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    void Update()
    {
        // Close with the Tower Manu, or when the tower is gone (sold, or the level reloaded).
        if (!Exists(tower) || menu == null || !menu.activeInHierarchy)
        {
            Hide();
            return;
        }

        // A tower that was just upgraded only gets its shop bonus in Start, a frame after we opened,
        // so keep re-reading a few times a second instead of only once.
        if (Time.unscaledTime >= nextRefresh) Refresh();
    }

    void Refresh()
    {
        nextRefresh = Time.unscaledTime + refreshInterval;

        string towerName = tower.DisplayName;
        if (string.IsNullOrEmpty(towerName)) towerName = tower.transform.name.Replace("(Clone)", "").Trim();
        SetText(nameText, towerName);

        // How many tiers this line has: this one plus every upgrade still ahead of it.
        int level = tower.Level;
        int maxLevel = level;
        GameObject next = tower.UpgradePrefab;
        while (next != null && maxLevel < 50)   // 50 = a safety stop in case two prefabs upgrade into each other
        {
            ++maxLevel;
            ITower nextTier = next.GetComponent<ITower>();
            next = nextTier != null ? nextTier.UpgradePrefab : null;
        }
        SetText(levelText, level >= maxLevel ? $"Level {level} / {maxLevel} (Max)" : $"Level {level} / {maxLevel}");

        SetText(descriptionText, tower.Description ?? "");

        // One stat per line: label on the left, value lined up in a column (TMP's <pos> tag).
        stats.Clear();
        tower.ListStats(stats);
        lines.Clear();
        string column = "<pos=" + Mathf.RoundToInt(valueColumn * 100f) + "%><b>";
        for (int i = 0; i < stats.Count; ++i)
        {
            if (i > 0) lines.Append('\n');
            lines.Append(stats[i].label).Append(column).Append(stats[i].value).Append("</b>");
        }
        SetText(statsText, lines.ToString());
    }

    // Only touch the text when it changed, so TMP doesn't rebuild its mesh four times a second for nothing.
    static void SetText(TextMeshProUGUI label, string text)
    {
        if (label != null && label.text != text) label.text = text;
    }
}
