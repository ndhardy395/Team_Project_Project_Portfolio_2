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

    private List<Transform> Buttons = new List<Transform>();
    private GameObject Manu;

    private int towerCost;
    private Transform towerTransform;
    private TextMeshProUGUI TowerText ;

    //private levelClick Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Instance = this;
        Manu = BaseManu;

        for (int i = 0; i < ClickCode.Instance.GetNumberOfPrfabs(); i++)
        {
            int number = i + 1;
            string stringtemp = "tower " + number.ToString() + "/" + "Cost " + number.ToString();

            Transform temp = Manu.transform.Find(stringtemp);
        

            Buttons.Add(temp);
        }
       
    }

    // Update is called once per frame
    void Update()
    {
    //    if (Manu.activeSelf)
    //    {
    //        for (int i = 0; i < Buttons.Count; i++)
    //        {
    //            towerTransform = Buttons[1];
    //            ////tempGame.transform=temp;
    //            TextMeshProUGUI tempText = new TextMeshProUGUI();
    //            tempText = towerTransform.GetComponent<TextMeshProUGUI>();// tempGame.GetComponentInChildren<TextMeshProUGUI>();
    //            string text = tempText.text;
    //            string digitsOnly = "100";
    //            digitsOnly = Regex.Replace(text, @"\D", "");
    //            int cost = 0;
    //            int.TryParse(digitsOnly, out cost);
    //            //if ()
    //            //{

    //            //}
    //            //else
    //            //{

    //            //}

    //        }
    //    }

    }

    public void BackButton()
    {
        BaseManu.SetActive(false);
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
            Debug.Log(cost + " was clicked in the game world!");
            BaseManu.SetActive(false);
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
