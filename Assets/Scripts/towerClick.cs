using UnityEngine;
using UnityEngine.EventSystems;
public class towerClick : MonoBehaviour
{
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
        // A click on a menu button shouldn't also select the tower behind it.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Debug.Log(gameObject.name + " was clicked in the game world!");
        // The Tower Manu lives in the UI scene, so the prefab can't drag it in; look it up instead.
        TowerUpgrad.Find().Select(GetComponent<Gun>());
    }
}
