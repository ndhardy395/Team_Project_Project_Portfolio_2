using UnityEngine;
using UnityEngine.Events;

namespace Warplane
{
    /// <summary>
    /// The warplane's health. Enemy attacks call TakeDamage(amount): GetComponent&lt;WarplaneHealth&gt;().TakeDamage(10f),
    /// or SendMessage("TakeDamage", 10f) like your other targets.
    ///  - Health falls to 2/3: a wingtip breaks off (it flies on, leaning and wobbling a little).
    ///    At 1/3 the other one goes.
    ///  - Health 0: a wing or the tail tears off and it spirals down (WarplaneFlight), then explodes when it
    ///    hits the ground. If respawn is on, it comes back at its start after respawnDelay, good as new.
    /// Hook your game in through the events (score, sounds, UI) or read Health, IsDown and IsDead.
    /// </summary>
    [RequireComponent(typeof(WarplaneFlight))]
    public class WarplaneHealth : MonoBehaviour
    {
        public WarplaneSettings settings;
        [Tooltip("Hidden when it explodes (the 'Model' child).")]
        public GameObject model;
        [Tooltip("Switched off when it explodes, so nothing can hit the wreck (the 'Colliders' child).")]
        public GameObject colliders;
        public GameObject explosionPrefab;

        [Header("Events")]
        public UnityEvent onDamaged = new UnityEvent();
        [Tooltip("Health hit 0: a wing or the tail just tore off and it's going down.")]
        public UnityEvent onShotDown = new UnityEvent();
        [Tooltip("It hit the ground and exploded.")]
        public UnityEvent onExploded = new UnityEvent();
        public UnityEvent onRespawned = new UnityEvent();

        public float Health { get; private set; }
        public float MaxHealth => settings != null ? settings.maxHealth : 100f;
        /// <summary>Shot down: spiraling, or already exploded.</summary>
        public bool IsDown { get; private set; }
        /// <summary>Exploded (hidden), waiting to respawn.</summary>
        public bool IsDead { get; private set; }

        WarplaneFlight flight;
        PlaneStructure structure;
        float respawnAt = -1f;

        void Awake()
        {
            flight = GetComponent<WarplaneFlight>();
            structure = GetComponent<PlaneStructure>();
            Health = MaxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDown || amount <= 0f) return;
            Health = Mathf.Max(0f, Health - amount);
            onDamaged.Invoke();
            if (structure != null && settings != null)
            {
                float share = Health / MaxHealth;
                if (share <= settings.firstTipBreaksAt && TipsGone() < 1) BreakWingtip();
                if (share <= settings.secondTipBreaksAt && TipsGone() < 2) BreakWingtip();
            }
            if (Health <= 0f) ShotDown();
        }

        int TipsGone() => structure.TipsLost(-1) + structure.TipsLost(1);

        void BreakWingtip()
        {
            BreakablePart tip = PickIntact(PartKind.WingTip, 0);
            if (tip != null) structure.Break(tip, tip.transform.position, Outward(tip));
        }

        /// <summary>A wing (more often the side already missing its tip) or the tail tears off.</summary>
        void ShotDown()
        {
            IsDown = true;
            if (structure != null)
            {
                int weakSide = structure.TipsLost(1) > structure.TipsLost(-1) ? 1 : structure.TipsLost(-1) > 0 ? -1 : 0;
                BreakablePart part = Random.value < 0.25f ? PickIntact(PartKind.Tail, 0) : PickIntact(PartKind.Wing, weakSide);
                if (part == null) part = PickIntact(PartKind.Wing, 0);
                if (part != null) structure.Break(part, part.transform.position, Outward(part));
            }
            onShotDown.Invoke();
            if (structure == null || !structure.IsDoomed) Explode(transform.position);   // nothing to tear off: just explode
        }

        /// <summary>A random intact part of this kind; preferSide (+1/-1) wins if one is there.</summary>
        BreakablePart PickIntact(PartKind kind, int preferSide)
        {
            BreakablePart pick = null;
            int seen = 0;
            foreach (BreakablePart p in structure.Parts)
            {
                if (p.kind != kind || p.IsBroken || !p.gameObject.activeInHierarchy) continue;
                if (preferSide != 0 && p.side == preferSide) return p;
                if (Random.Range(0, ++seen) == 0) pick = p;   // each one equally likely
            }
            return pick;
        }

        Vector3 Outward(BreakablePart p) => (transform.right * (p.side != 0 ? p.side : 1) + transform.up).normalized;

        /// <summary>Boom: called by WarplaneFlight when the falling plane hits the ground.</summary>
        public void Explode(Vector3 at)
        {
            if (IsDead) return;
            IsDead = true;
            IsDown = true;
            Health = 0f;
            if (explosionPrefab != null)
            {
                GameObject fx = Instantiate(explosionPrefab, at, Quaternion.identity);
                SimpleExplosion se = fx.GetComponent<SimpleExplosion>();
                if (se != null && settings != null) se.size = settings.crashExplosionSize;
            }
            if (model != null) model.SetActive(false);
            if (colliders != null) colliders.SetActive(false);
            onExploded.Invoke();
            respawnAt = settings != null && settings.respawn ? Time.time + settings.respawnDelay : -1f;
        }

        void Update()
        {
            if (IsDead && respawnAt > 0f && Time.time >= respawnAt) Respawn();
        }

        /// <summary>Back at its start, whole and at full health.</summary>
        public void Respawn()
        {
            respawnAt = -1f;
            if (structure != null) structure.RestoreAll();
            if (model != null) model.SetActive(true);
            if (colliders != null) colliders.SetActive(true);
            Health = MaxHealth;
            IsDown = false;
            IsDead = false;
            flight.ResetToStart();
            WarplaneGuns guns = GetComponent<WarplaneGuns>();
            if (guns != null) guns.ResetHeat();
            WarplaneBombs bombs = GetComponent<WarplaneBombs>();
            if (bombs != null) bombs.RearmAll();
            WakeWisps wisps = GetComponent<WakeWisps>();
            if (wisps != null) wisps.Clear();
            onRespawned.Invoke();
        }
    }
}
