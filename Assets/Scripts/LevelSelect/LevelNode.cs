using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One stop on the level select map: a normal level, a key level, or the boss.
// The builder (Tools > Skyline Rush > Build Level Select) makes these and fills in the parts below.
// Refresh() reads the save file and picks the look: locked, open, next, or beaten.
// LevelSelectScreen moves the node every frame so it stays on top of its spot on the 3D map.
public class LevelNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum Kind { Level, Key, Boss }
    public enum State { Locked, Open, Next, Beaten }

    [Header("What this node is")]
    public LevelData level;
    public Kind kind = Kind.Level;
    [Tooltip("Small label above the name in the level popup, like LEVEL 1 or KEY LEVEL A.")]
    public string label = "LEVEL 1";
    [Tooltip("Text inside a normal level's circle.")]
    public string number = "1";
    [Tooltip("The point on the 3D map this node sits on.")]
    public Transform anchor;

    [Header("Parts (filled in by the builder)")]
    public Button button;
    public Image ring;          // the outer shape; its color is the border color
    public Image fill;          // sits inside the ring
    public Image dashedRing;    // key levels, while locked
    public Image icon;          // lock, key or castle
    public TMP_Text numberText;
    public RectTransform starRow;
    public Image[] stars;       // normal levels: 4 small stars, the last one is the Bloodlust star
    public RectTransform nextTag;
    public Image glowInner;
    public Image glowOuter;
    public Image[] locks;       // boss: one per key needed
    public TMP_Text caption;    // KEY EARNED, BOSS · 1/3 KEYS, COMING SOON

    [Header("Sprites")]
    public Sprite lockSprite;
    public Sprite lockOpenSprite;
    public Sprite starSprite;
    public Sprite starOutlineSprite;

    public State CurrentState { get; private set; }
    public RectTransform Rect => (RectTransform)transform;

    float hoverTarget = 1f;
    float hoverScale = 1f;
    bool comingSoon;   // locked only because this build doesn't include it yet, and the player has earned it

    public void Refresh(bool isNext)
    {
        bool unlocked = SaveManager.IsUnlocked(level);
        bool beaten = SaveManager.IsBeaten(level);
        comingSoon = level.comingSoon && SaveManager.RequirementsMet(level);

        if (!unlocked)
            CurrentState = State.Locked;
        else if (beaten)
            CurrentState = State.Beaten;
        else
            CurrentState = isNext ? State.Next : State.Open;

        button.interactable = unlocked;
        if (!unlocked)
            hoverTarget = 1f;

        switch (kind)
        {
            case Kind.Level: StyleLevel(); break;
            case Kind.Key: StyleKey(); break;
            case Kind.Boss: StyleBoss(); break;
        }
    }

    void StyleLevel()
    {
        bool locked = CurrentState == State.Locked;
        bool next = CurrentState == State.Next;

        // Sizes and border widths straight from the mockup: locked 48/2, next 64 solid gold, the rest 56/3.
        float size = locked ? 48f : next ? 64f : 56f;
        float border = locked ? 2f : next ? 0f : 3f;
        SetCircle(size, border,
            locked ? LevelSelectStyle.LockedBorder : LevelSelectStyle.Gold,
            locked ? LevelSelectStyle.LockedFill : next ? LevelSelectStyle.Gold : LevelSelectStyle.NodeFill);

        icon.gameObject.SetActive(locked);
        icon.sprite = lockSprite;
        icon.color = LevelSelectStyle.LockedIcon;

        numberText.gameObject.SetActive(!locked);
        numberText.text = number;
        numberText.fontSize = next ? 26f : 22f;
        numberText.color = next ? LevelSelectStyle.OnGold : LevelSelectStyle.Text;

        nextTag.gameObject.SetActive(next);
        nextTag.anchoredPosition = new Vector2(0f, size / 2f + 8f + 11f);
        glowInner.gameObject.SetActive(next);
        glowOuter.gameObject.SetActive(next);

        bool showStars = CurrentState == State.Beaten || CurrentState == State.Open;
        starRow.gameObject.SetActive(showStars);
        starRow.anchoredPosition = new Vector2(0f, -(size / 2f + 8f + 7.5f));
        if (showStars)
            LevelSelectStyle.PaintStars(stars, SaveManager.GetStars(level), starSprite, starOutlineSprite);

        // Where the stars would go. Null on a level select built before captions were added to normal levels.
        if (caption != null)
        {
            caption.gameObject.SetActive(comingSoon);
            caption.text = "COMING SOON";
            caption.color = LevelSelectStyle.TextMuted;
            caption.rectTransform.anchoredPosition = new Vector2(0f, -(size / 2f + 8f + 7f));
        }
    }

    void StyleKey()
    {
        bool locked = CurrentState == State.Locked;

        ring.gameObject.SetActive(!locked);
        dashedRing.gameObject.SetActive(locked);
        dashedRing.color = LevelSelectStyle.KeyLockedBorder;
        SetCircle(44f, 2f, LevelSelectStyle.Key, locked ? LevelSelectStyle.LockedFill : LevelSelectStyle.KeyFill);

        icon.color = locked ? LevelSelectStyle.KeyLockedIcon : LevelSelectStyle.Key;
        icon.rectTransform.sizeDelta = Vector2.one * (locked ? 18f : 20f);

        caption.gameObject.SetActive(!locked || comingSoon);
        caption.text = comingSoon ? "COMING SOON" : CurrentState == State.Beaten ? "KEY EARNED" : "KEY LEVEL";
        caption.color = comingSoon ? LevelSelectStyle.TextMuted : LevelSelectStyle.Key;
    }

    void StyleBoss()
    {
        bool locked = CurrentState == State.Locked;
        int keys = SaveManager.KeyCount();

        SetCircle(84f, locked ? 2f : 3f,
            locked ? LevelSelectStyle.BossBorder : LevelSelectStyle.Gold,
            CurrentState == State.Beaten ? LevelSelectStyle.NodeFill : LevelSelectStyle.LockedFill);
        icon.color = locked ? LevelSelectStyle.BossIcon : LevelSelectStyle.Gold;

        for (int i = 0; i < locks.Length; i++)
        {
            bool open = i < keys;
            locks[i].sprite = open ? lockOpenSprite : lockSprite;
            locks[i].color = open ? LevelSelectStyle.Key : LevelSelectStyle.LockedIcon;
        }

        if (CurrentState == State.Beaten)
            caption.text = "BOSS DEFEATED";
        else if (comingSoon)
            caption.text = "BOSS · COMING SOON";
        else if (locked)
            caption.text = $"BOSS · {Mathf.Min(keys, level.keysRequired)}/{level.keysRequired} KEYS";
        else
            caption.text = "BOSS · READY";
        caption.color = locked ? LevelSelectStyle.TextMuted : LevelSelectStyle.Gold;
    }

    // Sizes the ring and the fill inside it. A border of 0 makes the fill cover the ring completely.
    void SetCircle(float size, float border, Color borderColor, Color fillColor)
    {
        ring.rectTransform.sizeDelta = Vector2.one * size;
        ring.color = borderColor;
        fill.rectTransform.sizeDelta = Vector2.one * (size - border * 2f);
        fill.color = fillColor;
        if (dashedRing != null)
            dashedRing.rectTransform.sizeDelta = Vector2.one * size;
    }

    void Update()
    {
        // Menus run with Time.timeScale = 0, so animate with unscaled time.
        float dt = Time.unscaledDeltaTime;
        hoverScale = Mathf.MoveTowards(hoverScale, hoverTarget, dt * 1.5f);
        transform.localScale = Vector3.one * hoverScale;

        if (CurrentState == State.Next && glowInner != null)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * 3f) + 1f) * 0.5f;
            glowInner.rectTransform.localScale = Vector3.one * (1f + pulse * 0.06f);
            glowOuter.rectTransform.localScale = Vector3.one * (1f + pulse * 0.12f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
            hoverTarget = 1.08f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverTarget = 1f;
    }
}
