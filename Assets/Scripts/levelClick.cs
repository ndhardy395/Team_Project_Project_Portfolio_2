using UnityEngine;
using System.Collections.Generic;

public class levelClick : MonoBehaviour
{
    
    [SerializeField]
    private GameObject TowerHUD;
    public levelClick Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BackButton()
    {
        TowerHUD.SetActive(false);
    }
    public void Tower1Button()
    {
        TowerHUD.SetActive(false);
        ClickCode.Instance.SetTowerPrefab(1);
        ClickCode.Instance.OnTowerHUDTowerClick();
    }
}
