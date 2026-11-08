using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System.Text.RegularExpressions;
public class levelClick : MonoBehaviour
{


    private TextMeshProUGUI tempText;

    private List<Transform> Buttons = new List<Transform>();
    private GameObject Manu;

    private int towerCost;
    private Transform towerTransform;
    private TextMeshProUGUI TowerText;

    //private levelClick Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Manu = this.transform.gameObject;
        //Instance = this;
        for (int i = 0; i < ClickCode.Instance.GetNumberOfPrfabs(); i++)
        {
            int number = i + 1;
            string stringtemp = "tower " + number.ToString() + "/" + "Cost " + number.ToString();

            Transform temp = this.transform.Find(stringtemp);

            if (temp != null)
            {
                Buttons.Add(temp);
            }

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.gameObject.activeSelf)
        {
            for (int i = 0; i < Buttons.Count; i++)
            {
                towerTransform = Buttons[i];
                ////tempGame.transform=temp;


                TowerText = towerTransform.GetComponent<TextMeshProUGUI>();// tempGame.GetComponentInChildren<TextMeshProUGUI>();
                string text = TowerText.text;
                string digitsOnly;
                digitsOnly = Regex.Replace(text, @"\D", "");
                int cost = 0;
                int.TryParse(digitsOnly, out cost);
                if (cost <= CurrencyManager.instance.currentCurrency) // this would make button turn green when avaliable
                {
                    TowerText.color = Color.darkGreen;
                }

                else// if user don't have enough money show red
                {
                    TowerText.color = Color.red;
                }


            }
        }

    }

    public void BackButton()
    {
        this.transform.gameObject.SetActive(false);
    }
    public void Tower1Button()
    {
        BuyTower("tower 1/Cost 1", 2);
    }

    public void Tower2Button()
    {
        BuyTower("tower 2/Cost 2", 3);
    }

    public void Tower3Button()
    {
        BuyTower("tower 3/Cost 3", 4);
    }

    public void Tower4Button()
    {
        BuyTower("tower 4/Cost 4", 5);
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
        Debug.Log(cost);
        if (cost <= money)
        {
            ClickCode.Instance.SetTowerPrefab(towerNumber);
            ClickCode.Instance.OnTowerHUDTowerClick();
            CurrencyManager.instance.SubCurrency(cost);
            this.transform.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("not enuf money");
        }

        //GameObject tempGame = new GameObject();






    }

    public void SetTextColor(TextMeshProUGUI text, int R, int G, int B, int A)
    {

    }
}
