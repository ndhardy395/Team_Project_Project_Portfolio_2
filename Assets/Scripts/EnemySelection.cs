using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EnemySelection : MonoBehaviour
{
    public Camera gameCamera;
    public EnemyInfoUI enemyInfoUI;

    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    private void Update()
    {
        Vector2 pointPosition;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        pointPosition = Mouse.current.position.ReadValue();
#else
        if (!Input.GetMouseButtonDown(0))
            return;

        pointPosition = Input.mousePosition;
#endif

        if (enemyInfoUI == null || !enemyInfoUI.isActiveAndEnabled || IsOverUIControl(pointPosition))
            return;

        Camera selectionCamera = gameCamera != null && gameCamera.isActiveAndEnabled ? gameCamera : Camera.main;
        if (selectionCamera == null)
            return;

        Ray ray = selectionCamera.ScreenPointToRay(pointPosition);
        EnemyHealthDisplay enemy = null;
        float nearestDistance = float.PositiveInfinity;

        // Enemy hitboxes can be triggers or live on child objects. Ignore
        // unrelated hits such as spell effects when looking for the closest enemy.
        foreach (RaycastHit hit in Physics.RaycastAll(ray, Mathf.Infinity,
                     Physics.AllLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.distance >= nearestDistance)
                continue;

            EnemyHealthDisplay candidate = hit.collider.GetComponentInParent<EnemyHealthDisplay>();
            if (candidate == null || candidate.IsDead || !candidate.isActiveAndEnabled)
                continue;

            enemy = candidate;
            nearestDistance = hit.distance;
        }

        enemyInfoUI.SelectEnemy(enemy, selectionCamera);
    }

    private bool IsOverUIControl(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        uiHits.Clear();
        eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = position }, uiHits);

        // Decorative HUD graphics can receive raycasts even when they handle no input.
        foreach (RaycastResult hit in uiHits)
        {
            if (!(hit.module is GraphicRaycaster))
                continue;

            GameObject target = hit.gameObject;
            return ExecuteEvents.GetEventHandler<IPointerDownHandler>(target) != null ||
                   ExecuteEvents.GetEventHandler<IPointerClickHandler>(target) != null ||
                   ExecuteEvents.GetEventHandler<IDragHandler>(target) != null;
        }

        return false;
    }
}
