using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EnemySelection : MonoBehaviour
{

    public Camera gameCamera;
    public EnemyInfoUI enemyInfoUI;


    // Update is called once per frame
    void Update()
    {
        Vector2 pointPosition;

#if ENABLE_INPUT_SYSTEM

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        pointPosition = Mouse.current.position.ReadValue();


#else
if(!Input.GetMouseButtonDown(0))
{
return;
}

pointPosition = Input.mousePosition;
#endif

        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if(gameCamera == null || enemyInfoUI == null)
        {
            return;
        }

        Ray ray = gameCamera.ScreenPointToRay(pointPosition); // this makes an imaginary line through the spot u click on screen
        EnemyHealthDisplay enemy = null;

        if(Physics.Raycast(ray, out RaycastHit hit)) 

        {
            enemy = hit.collider.GetComponentInParent<EnemyHealthDisplay>();
        }

        enemyInfoUI.SelectEnemy(enemy);
    }
}