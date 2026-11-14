using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BassClick : MonoBehaviour
{


    private GameObject BaseManu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private int menuOpenedFrame = -1;
    void Start()
    {
        BaseManu = ClickCode.Instance.GetBaseManu();
    }

    // Update is called once per frame
    void Update()
    {
       if(BaseManu == null)
        {
            return;
        }

        Transform cube = transform.Find("Cube");

        if(cube == null )
        {
            return;
        }

        Renderer renderColor = cube.GetComponent<Renderer>();

        if(renderColor == null || renderColor.material.color != Color.blue)
        {
            return;
        }

        if(!BaseManu.activeSelf)
        {
            renderColor.material.color = Color.red;

            if(ClickCode.Instance != null)
            {
                ClickCode.Instance.SetTowerSpace(null);
                return;
            }
        }

        // Don't close the menu on the click that opened it.

        if (Time.frameCount == menuOpenedFrame || Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        Canvas canvas = BaseManu.GetComponent<Canvas>();

        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        RectTransform menuRect = BaseManu.GetComponent<RectTransform>();

        // Clicking inside the menu leaves it button usable 

        if(menuRect == null || RectTransformUtility.RectangleContainsScreenPoint(menuRect,Mouse.current.position.ReadValue(),uiCamera))
        {
            return;
        }

        BaseManu.SetActive(false);
        renderColor.material.color = Color.red;

        if(ClickCode.Instance !=null)
        {
            ClickCode.Instance.SetTowerSpace(null);
        }
    }

    public void OnMouseDown()
    {
        // Ui clicks shouldn't also select the base underneath.

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if(BaseManu == null || ClickCode.Instance == null)
        {
            return;
        }

        menuOpenedFrame = Time.frameCount;

        // clear any previous base selection before slecting this one.
        foreach(BassClick towerbase in FindObjectsByType<BassClick>())
        {
            Transform otherCube = towerbase.transform.Find("Cube");

            if(otherCube == null)
            {
                continue;
            }

            Renderer otherRenderer = otherCube.GetComponent<Renderer>();

            if(otherRenderer != null && otherRenderer.material.color == Color.blue)
            {
                otherRenderer.material.color = Color.red;
            }
        }

        TowerUpgrad upgradePanel = TowerUpgrad.Find();

        if(upgradePanel!= null)
        {
            upgradePanel.BackButton();
        }

        BaseManu.SetActive(true);
        ClickCode.Instance.SetTowerSpace(gameObject);

        Transform cube = transform.Find("Cube");

        if(cube != null && cube.TryGetComponent(out Renderer renderColor))
        {
            renderColor.material.color = Color.blue;
        }

      
    }



}
