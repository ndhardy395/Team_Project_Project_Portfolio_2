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
            renderColor.material.color = new Color32(164, 48, 0,255);

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

        // GetComponentsInChildren returns all menu buttons.
        UnityEngine.UI.Button[] menuButtons =
            BaseManu.GetComponentsInChildren<UnityEngine.UI.Button>();

        foreach (UnityEngine.UI.Button button in menuButtons)
        {
            UnityEngine.Canvas menuCanvas =
                button.GetComponentInParent<UnityEngine.Canvas>();

            UnityEngine.Canvas rootCanvas =
                menuCanvas != null ? menuCanvas.rootCanvas : null;

            Camera uiCamera = null;

            if (rootCanvas != null &&
                rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = rootCanvas.worldCamera;
            }

            RectTransform buttonRect = button.transform as RectTransform;

            if (buttonRect != null &&
                RectTransformUtility.RectangleContainsScreenPoint(
                    buttonRect,
                    Mouse.current.position.ReadValue(),
                    uiCamera))
            {
                // Let the button handle this click.
                return;
            }
        }

        // Clicked outside all purchase buttons.
        BaseManu.SetActive(false);
        renderColor.material.color = new Color32(164, 48, 0, 255);

        if (ClickCode.Instance != null)
            ClickCode.Instance.SetTowerSpace(null);
    }

    public void OnMouseDown()
    {
        // Ui clicks shouldn't also select the base underneath.

        if (EventSystem.current != null)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);

            foreach (RaycastResult result in results)
            {
                if (result.module is UnityEngine.UI.GraphicRaycaster)
                {
                    Debug.LogWarning(
                        $"UI blocking pad: {result.gameObject.name}",
                        result.gameObject);
                    return;
                }
            }
        }

        if (BaseManu == null || ClickCode.Instance == null)
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
                otherRenderer.material.color = new Color32(164, 48, 0, 255);
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
