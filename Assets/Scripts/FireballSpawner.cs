using UnityEngine;
using UnityEngine.InputSystem;

public class SpellSpawner : MonoBehaviour
{
    public GameObject fireballPrefab;

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Read mouse position using new Input System. Old gave errors
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 spawnPosition = hit.point + new Vector3(0f, 10f, 0f);
                Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }
}
