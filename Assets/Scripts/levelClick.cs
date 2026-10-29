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
        //Instance = this;
        for (int i = 0; i < ClickCode.Instance.GetNumberOfPrfabs(); i++)
        {
            int number = i + 1;
            string stringtemp = "tower " + number.ToString() + "/" + "Cost " + number.ToString();

            Transform temp = this.transform.Find(stringtemp);


            Buttons.Add(temp);
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
                if (towerCost <= CurrencyManager.instance.currentCurrency)
                {

                     TowerText.faceColor = Color.black;
                    Debug.Log("no color" + CurrencyManager.instance.currentCurrency);
                }
                else
                {
                    TowerText.faceColor = new Color32(255, 255, 0, 255);

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
        Transform temp = Buttons[0];
        ////tempGame.transform=temp;
        TextMeshProUGUI tempText = new TextMeshProUGUI();
        tempText = temp.GetComponent<TextMeshProUGUI>();// tempGame.GetComponentInChildren<TextMeshProUGUI>();
        string text = tempText.text;
        string digitsOnly = "100";
        digitsOnly = Regex.Replace(text, @"\D", "");
        int cost = 0;
        int.TryParse(digitsOnly, out cost);
        int money = CurrencyManager.instance.currentCurrency;
        Debug.Log(cost);
        if (cost <= money)
        {
            ClickCode.Instance.SetTowerPrefab(2);
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
