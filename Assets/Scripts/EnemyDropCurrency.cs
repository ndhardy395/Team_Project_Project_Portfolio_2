using UnityEngine;

public class EnemyDropCurrency : MonoBehaviour
{

    public GameObject itemToDrop;

    public int currencyAmount = 5;

    private bool hasDropped;

    public void DropItem()
    {
        if (hasDropped || itemToDrop == null) return;

        hasDropped = true;

        GameObject coin = Instantiate(itemToDrop, transform.position, Quaternion.identity);

        CurrencyPickUp pickup = coin.GetComponent<CurrencyPickUp>();

        if(pickup != null)
        {
            pickup.coinValue = currencyAmount;
        }
    }

    
}
