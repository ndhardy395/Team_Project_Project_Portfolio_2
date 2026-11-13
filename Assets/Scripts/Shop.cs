using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class Shop : MonoBehaviour
{
    [SerializeField] private TMP_Text starsText;

    [SerializeField] private Button fireButton;
    [SerializeField] private TMP_Text fireButtonText;


    [SerializeField] private Button iceButton;
    [SerializeField] private TMP_Text iceButtonText;

    [SerializeField] private Button lightningButton;
    [SerializeField] private TMP_Text ligntningButtonText;
    [SerializeField] private TMP_Text spellsStarsText;

    // Shop Pages
    [SerializeField] private GameObject turrrentPage;
    [SerializeField] private GameObject spellsPage;
    [SerializeField] private Button leftArrow;
    [SerializeField] private Button RightArrow;

    // turrent upgrades

    [SerializeField] private Button turrentUpgradeButton;
    [SerializeField] private TMP_Text turrentUpgradeButtonText;

    [SerializeField] private Button aoeUpgradeButton;
    [SerializeField] private TMP_Text aoeUpgradeButtonText;

    [SerializeField] private Button spellTowerUpgradeButton;
    [SerializeField] private TMP_Text spellTowerUpgradeButtonText;

    [SerializeField] private Button landMineTowerUpgradeButton;
    [SerializeField] private TMP_Text landMineTowerUpgradeButtonText;
    public void ShowTurrets()
    {
        turrrentPage.SetActive(true);
        spellsPage.SetActive(false);

        leftArrow.interactable = false;
        RightArrow.interactable = true;
    }

    public void ShowSpells()
    {
        turrrentPage.SetActive(false);
        spellsPage.SetActive(true);

        leftArrow.interactable = true;
        RightArrow.interactable = false;
    }

    private void OnEnable()
    {
        ShowTurrets();
        RefreshUI();
    }

    public void BuyFire()
    {
        SaveManager.TryUnlockSpell("fireball");
        RefreshUI();
    }
    public void BuyIce()
    {
        SaveManager.TryUnlockSpell("ice");
        RefreshUI();
    }
    public void BuyLightning()
    {
        SaveManager.TryUnlockSpell("lightning");
        RefreshUI();
    }
    private void RefreshUI()
    {
        int stars = SaveManager.AvaliableStars();

        starsText.text = $"Stars: {stars}";

        spellsStarsText.text = $"Stars: {stars}";


        UpdateSpellButton("fireball", "Fire", 0, stars, fireButton, fireButtonText);
        UpdateSpellButton("ice", "Ice", 2, stars, iceButton, iceButtonText);
        UpdateSpellButton("lightning", "Lightning", 4, stars, lightningButton, ligntningButtonText);
        UpdateTowerButton("turret", turrentUpgradeButton, turrentUpgradeButtonText, stars);
        UpdateTowerButton("aoe", aoeUpgradeButton, aoeUpgradeButtonText, stars);
        UpdateTowerButton("spellTower", spellTowerUpgradeButton, spellTowerUpgradeButtonText, stars);
        UpdateTowerButton("landMine", landMineTowerUpgradeButton, landMineTowerUpgradeButtonText, stars);

    }

    private void UpdateSpellButton(string spellID, string spellName, int cost, int stars, Button button, TMP_Text buttonText)
    {
        bool owned = SaveManager.IsSpellUnlocked(spellID);

        button.interactable = !owned && stars >= cost;

        if (owned)
        {
            buttonText.text = $"{spellName} - Owned";
        }
        else if (stars < cost)
        {
            buttonText.text = $"{spellName} - Need {cost - stars} more stars";

        }
        else
        {
            buttonText.text = $"Buy\n{spellName}\n{cost} stars";

        }
    }

    public void BuyTurretUpgrade()
    {
        SaveManager.TryUpgradeTower("turret");
        RefreshUI();
    }

    public void BuyAOEUpgrade()
    {
        SaveManager.TryUpgradeTower("aoe");
        RefreshUI();
    }

    public void BuySpellTowerUpgrade()
    {
        SaveManager.TryUpgradeTower("spellTower");
        RefreshUI();
    }

    public void BuyLandMineTowerUpgrade()
    {
        SaveManager.TryUpgradeTower("landMine");
        RefreshUI();
    }

    public void UpdateTowerButton(string towerId, Button button, TMP_Text buttonText, int stars)
    {

        if (button == null || buttonText == null)
        {
            return;
        }

        int upgrades = SaveManager.GetTowerUpgrades(towerId);
        int cost = SaveManager.GetTowerUpgradesCost(towerId);

        bool maxed = upgrades >= 2;
        bool canBuy = !maxed && cost > 0 && stars >= cost;

        button.interactable = canBuy;

        if(maxed)
        {
            buttonText.text = "Max";
        }
        else if(cost <= 0)
        {
            buttonText.text = "Unavaliable";
        }
        else if(stars < cost)
        {
            int missingStars = cost - stars;

            buttonText.text = $"Need {missingStars} more " + (missingStars == 1 ? "star" : "stars");
        }
        else
        {
            buttonText.text = $"Upgrade · {cost} " + (cost == 1 ? "star" : "stars");
        }
    }

    public void BackToMainMenu()
    {
        if (ClickCode.Instance != null)
        {
            ClickCode.Instance.ReturnToMainMenu();   // shows Main panel, then loads "Main Game and UI"
        }
        else
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main Game and UI");
        }
    }
}
