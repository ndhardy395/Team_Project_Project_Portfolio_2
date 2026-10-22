using System.Collections;
using UnityEngine;

public class CurrencyPickUp : MonoBehaviour
{
    public int coinValue = 1;

    public float pickupDelay = 3f;

    private IEnumerator Start()
    {
        Debug.Log($"Collecting{gameObject.name}, value: {coinValue}");
        yield return new WaitForSeconds(pickupDelay);

        if(CurrencyManager.instance != null)
        {
            CurrencyManager.instance.AddCurrency(coinValue);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("No active CurrencyManager found!");
        }
    }
    public int GetCoinValue()
    {
        return coinValue;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  

    // Update is called once per frame
    void Update()
    {
        
    }
}
