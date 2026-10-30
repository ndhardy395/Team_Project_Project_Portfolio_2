using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Sits on the Tower Manu panel. When the player clicks a placed tower, towerClick hands
// that tower to Select(): we show its range ring and offer its next tier (Gun_1 -> Gun_2).
public class TowerUpgrad : MonoBehaviour
{
    static TowerUpgrad panel;

    Gun selected;
    Button upgradeButton;
    TextMeshProUGUI upgradeCostText;

    // The panel starts hidden, and Awake doesn't run on hidden objects,
    // so towers find it through here instead of an Instance set in Awake.
    public static TowerUpgrad Find()
    {
        if (panel == null)
            panel = FindAnyObjectByType<TowerUpgrad>(FindObjectsInactive.Include);
        return panel;
    }

    void Awake()
    {
        // Hooked up by name (like levelClick finds its cost text), so don't also add these in the Inspector's OnClick.
        upgradeButton = transform.Find("Upgrade").GetComponent<Button>();
        upgradeButton.onClick.AddListener(UpgradeButton);
        transform.Find("Back").GetComponent<Button>().onClick.AddListener(BackButton);
        upgradeCostText = transform.Find("Upgrade Cost").GetComponent<TextMeshProUGUI>();
    }

    // Only runs while the panel is open: clicking anywhere in the world that isn't a tower closes it.
    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;   // our own buttons

        // A tower click was already handled by its towerClick (OnMouseDown runs before Update).
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetComponentInParent<towerClick>() != null) return;

        BackButton();
    }

    public void Select(Gun tower)
    {
        if (selected != null && selected != tower) selected.ShowRange(false);
        selected = tower;
        selected.ShowRange(true);

        gameObject.SetActive(true);   // runs Awake the first time, so the fields below are ready

        bool canUpgrade = selected.upgradePrefab != null;
        upgradeButton.interactable = canUpgrade;
        upgradeCostText.text = canUpgrade ? "Cost: " + selected.upgradeCost : "Max";
    }

    public void UpgradeButton()
    {
        if (selected == null || selected.upgradePrefab == null) return;

        int cost = selected.upgradeCost;
        if (cost > CurrencyManager.instance.currentCurrency)
        {
            Debug.Log("not enuf money");
            return;
        }
        CurrencyManager.instance.SubCurrency(cost);

        // The next tier is its own prefab (own model, barrels and stats), so swap the whole tower in place.
        Transform old = selected.transform;
        Gun upgraded = Instantiate(selected.upgradePrefab, old.position, old.rotation);
        Destroy(old.gameObject);

        selected = null;
        Select(upgraded);   // stay open on the new tower so the player sees its new range
    }

    public void BackButton()
    {
        if (selected != null) selected.ShowRange(false);
        selected = null;
        gameObject.SetActive(false);
    }
}
