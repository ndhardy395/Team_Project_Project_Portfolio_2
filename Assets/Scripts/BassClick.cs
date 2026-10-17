using UnityEngine;

public class BassClick : MonoBehaviour
{

    [SerializeField] private GameObject TowerManu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        Debug.Log(gameObject.name + " was clicked in the game world!");
        TowerManu.SetActive(true);
    }



}
