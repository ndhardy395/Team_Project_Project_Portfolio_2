using UnityEngine;
using UnityEngine.InputSystem;

namespace Warplane
{
    /// <summary>
    /// Keys for trying the warplane out before your targeting scripts exist; remove it once they do.
    /// No mouse clicks, so it doesn't fight the click-to-drop fireballs.
    ///  F (hold): fire the guns at the ground under the mouse cursor.
    ///  B: drop a bomb.
    ///  K: hit the plane for damagePerHit (at 25: the 2nd hit breaks a wingtip, the 3rd the other, the 4th shoots it down).
    /// </summary>
    public class WarplaneTestControls : MonoBehaviour
    {
        public float damagePerHit = 25f;

        WarplaneGuns guns;
        WarplaneBombs bombs;
        WarplaneHealth health;

        void Awake()
        {
            guns = GetComponent<WarplaneGuns>();
            bombs = GetComponent<WarplaneBombs>();
            health = GetComponent<WarplaneHealth>();
        }

        void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (kb.fKey.isPressed && guns != null && Camera.main != null && Mouse.current != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, ~0, QueryTriggerInteraction.Ignore) &&
                    !hit.collider.transform.IsChildOf(transform))
                    guns.FireAt(hit.point);
            }
            if (kb.bKey.wasPressedThisFrame && bombs != null) bombs.Drop();
            if (kb.kKey.wasPressedThisFrame && health != null) health.TakeDamage(damagePerHit);
        }
    }
}
