using Unity.VisualScripting;
using UnityEngine;

public class BassClick : MonoBehaviour
{

    [SerializeField] private GameObject BaseManu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.Find("Cube").GetComponent<Renderer>().material.color == Color.blue)
        {
            if (!BaseManu.activeSelf)
            {
                Transform child = transform.Find("Cube");
                Renderer renderColor = child.GetComponent<Renderer>();
                renderColor.material.color = Color.red;
            }
        }
    }

    public void OnMouseDown()
    {

        Debug.Log(gameObject.name + " was clicked in the game world!");
        BaseManu.SetActive(true);
        ClickCode.Instance.SetTowerSpace(this.gameObject);
        TowerUpgrad.Find().BackButton();   // close the tower menu so the two menus don't stack
        Transform child = transform.Find("Cube");
        Renderer renderColor = child.GetComponent<Renderer>();
        renderColor.material.color = Color.blue;
    }



}
