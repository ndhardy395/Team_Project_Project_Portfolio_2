using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Runs the level select: the 3D map, the dashed trails, the level nodes on top of it, and the popup.
// It lives in the UI scene (inside LevelSelect.prefab) and shows itself whenever ClickCode's Level Menu is open,
// so Logon's menu flow (Main > Level, Pause > Leave, Back) keeps working unchanged.
// Without a ClickCode in the scene (dropping the prefab into an empty scene to test it) it just stays visible.
public class LevelSelectScreen : MonoBehaviour
{
    [Header("3D map")]
    [SerializeField] GameObject world;
    [SerializeField] Camera mapCamera;
    [SerializeField] LevelTrail[] trails;
    [SerializeField] Material roadLit;
    [SerializeField] Material roadUnlit;
    [SerializeField] Material keyRoadLit;
    [SerializeField] Material keyRoadUnlit;

    [Header("Screen UI")]
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform nodeLayer;
    [Tooltip("In play order: the first open, unbeaten normal level in this list gets the NEXT tag.")]
    [SerializeField] LevelNode[] nodes;
    [SerializeField] LevelDetailsPopup popup;
    [SerializeField] Button backButton;
    [SerializeField] Button shopButton;
    [SerializeField] TMP_Text starsText;
    [SerializeField] TMP_Text keysText;
    [SerializeField] int keysForBoss = 3;

    bool shown;

    void Awake()
    {
        backButton.onClick.AddListener(OnBack);
        shopButton.onClick.AddListener(OnShop);
        foreach (LevelNode node in nodes)
        {
            LevelNode clicked = node;
            node.button.onClick.AddListener(() => popup.Open(clicked));
        }

        popup.Hide();
        Show(ShouldShow());
    }

    void Update()
    {
        bool open = ShouldShow();
        if (open != shown)
            Show(open);
    }

    // LateUpdate, so the nodes follow the camera after anything that moved it this frame.
    void LateUpdate()
    {
        if (shown)
            PlaceNodes();
    }

    bool ShouldShow()
    {
        // ClickCode sets Instance in its Start, so on the very first frame look for it directly.
        ClickCode menus = ClickCode.Instance != null ? ClickCode.Instance : FindAnyObjectByType<ClickCode>();
        return menus == null || menus.IsLevelMenuOpen;
    }

    void Show(bool show)
    {
        shown = show;
        world.SetActive(show);
        canvas.gameObject.SetActive(show);
        if (show)
        {
            Refresh();
            PlaceNodes();
        }
        else
        {
            popup.Hide();
        }
    }

    // Re-reads the save file. Called every time the screen opens.
    public void Refresh()
    {
        LevelNode next = null;
        foreach (LevelNode node in nodes)
        {
            if (node.kind == LevelNode.Kind.Level && SaveManager.IsUnlocked(node.level) && !SaveManager.IsBeaten(node.level))
            {
                next = node;
                break;
            }
        }

        foreach (LevelNode node in nodes)
            node.Refresh(node == next);

        foreach (LevelTrail trail in trails)
        {
            if (trail.keyBranch)
                trail.Refresh(keyRoadLit, keyRoadUnlit);
            else
                trail.Refresh(roadLit, roadUnlit);
        }

        // Stars you can still spend in the shop, since this sits right next to the Shop button.
        starsText.text = $"<b>{SaveManager.AvaliableStars()}</b>";
        keysText.text = $"<b>{SaveManager.KeyCount()}</b><color=#7F92A8> / {keysForBoss}</color>";
    }

    // Keeps each node on top of its spot on the 3D map, whatever the window size.
    void PlaceNodes()
    {
        foreach (LevelNode node in nodes)
        {
            Vector3 screenPoint = mapCamera.WorldToScreenPoint(node.anchor.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(nodeLayer, screenPoint, null, out Vector2 local);
            node.Rect.anchoredPosition = local;
        }
    }

    void OnBack()
    {
        if (ClickCode.Instance != null)
            ClickCode.Instance.OnLevelBackClick();
    }

    void OnShop()
    {
        if (ClickCode.Instance != null)
            ClickCode.Instance.OpenShop();
        else
            SceneManager.LoadScene("Shop");
    }
}
