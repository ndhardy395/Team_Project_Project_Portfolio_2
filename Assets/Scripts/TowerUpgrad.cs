using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
public class TowerUpgrad : MonoBehaviour
{

    //CurrencyManager;
    int SpeedCost=0;
    int DamgeCost=0;
    int RangCost=0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform temp = this.transform.Find("Speed Cost");
        TextMeshProUGUI tempText = temp.GetComponent<TextMeshProUGUI>();

        string stringtemp = Regex.Replace(tempText.text, @"\D", "");
        int.TryParse(stringtemp, out SpeedCost);

        temp = this.transform.Find("Damge Cost");
        tempText = temp.GetComponent<TextMeshProUGUI>();
        stringtemp = Regex.Replace(tempText.text, @"\D", "");
        int.TryParse(stringtemp, out DamgeCost);

        temp = this.transform.Find("Rang Cost");
        tempText = temp.GetComponent<TextMeshProUGUI>();
        stringtemp = Regex.Replace(tempText.text, @"\D", "");
        int.TryParse(stringtemp, out RangCost);

        Debug.Log(SpeedCost+" "+ DamgeCost+" "+ RangCost);

    }

    // Update is called once per frame
    void Update()
    {
        if (CurrencyManager.instance.currentCurrency>= SpeedCost)
        {
            Transform temp = this.transform.Find("Speed Cost");
            TextMeshProUGUI tempText = temp.GetComponent<TextMeshProUGUI>();
            tempText.color = new Color32(255, 255, 255, 255);
        }
    }
}
