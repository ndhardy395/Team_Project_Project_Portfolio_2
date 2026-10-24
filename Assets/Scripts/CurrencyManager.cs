using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
  public static CurrencyManager instance;

    public int currentCurrency = 0;

    public TMP_Text currencyText;



    private void Awake()
    {
        if (instance == null)
        {

            instance = this;

            // 2. Tell Unity not to destroy this object when loading a new scene
            //DontDestroyOnLoad(gameObject);
        }
        // 3. If an instance already exists, destroy this duplicate copy immediately
        else
        {
            Destroy(gameObject);
        }
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
    public void SubCurrency(int amount)
    {
        currentCurrency -= amount;

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
