using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System.Text.RegularExpressions;
public class levelClick : MonoBehaviour
{


    [SerializeField]
    private GameObject BaseManu;
    private TextMeshProUGUI tempText;

    private GameObject Manu;
    //private levelClick Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Instance = this;
        Manu = BaseManu;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BackButton()
    {
        BaseManu.SetActive(false);
    }
    public void Tower1Button()
    {
        BuyTower("tower 1/Cost 1", 2);
    }

    public void Tower2Button()
    {
        BuyTower("tower 2/Cost 2", 3);
    }

    // Reads the price off the menu's cost label, then places ClickCode's TowerPrefabs[towerNumber - 1] on the clicked base.
    void BuyTower(string costLabelPath, int towerNumber)
    {
        Transform temp = Manu.transform.Find(costLabelPath);
        TextMeshProUGUI tempText = temp.GetComponent<TextMeshProUGUI>();
        string text = tempText.text;
        string digitsOnly = "100";
        digitsOnly = Regex.Replace(text, @"\D", "");
        int cost = 0;
        int.TryParse(digitsOnly, out cost);
        int money = CurrencyManager.instance.currentCurrency;
        if (cost <= money)
        {
            ClickCode.Instance.SetTowerPrefab(towerNumber);
            ClickCode.Instance.OnTowerHUDTowerClick();
            CurrencyManager.instance.SubCurrency(cost);
            Debug.Log(cost + " was clicked in the game world!");
            BaseManu.SetActive(false);
        }
        else
        {
            Debug.Log("not enuf money");
        }

        //GameObject tempGame = new GameObject();






    }
}
