using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// A broken-off piece falling away. Its collider switches on after a moment and never touches the plane
    /// it came from. Removed after its lifetime.
    /// </summary>
    public class Debris : MonoBehaviour
    {
        Collider box;
        Collider[] ignore;
        float armTime;

        public void Init(Collider collider, Collider[] ignoreThese, float armDelay, float lifetime)
        {
            box = collider;
            ignore = ignoreThese;
            armTime = Time.time + armDelay;
            box.enabled = false;
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            if (box == null || box.enabled || Time.time < armTime) return;
            box.enabled = true;
            // Unity refuses switched-off colliders here, and those can't touch it anyway.
            foreach (Collider c in ignore)
                if (c != null && c.enabled && c.gameObject.activeInHierarchy) Physics.IgnoreCollision(box, c);
        }
    }
}
