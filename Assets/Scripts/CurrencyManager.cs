using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
  public static CurrencyManager instance;

    public int currentCurrency = 0;

    public TMP_Text currencyText;



    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateCurrencyUI();
    }

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;

        UpdateCurrencyUI();

        Debug.Log("Current Currency" + currentCurrency);
    }

    private void UpdateCurrencyUI()
    {
        if(currencyText != null)
        {
            currencyText.text = "Coins: " + currentCurrency;
        }
    }
}
