using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
public class SpellSpawner : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Button fireBallButton;
    public TMP_Text coolDownText;
    public float cooldownDuration = 10f;
    public float spawnHeight = 10f;

    // choose only the map/ ground layers in the Inspectoer.

    public LayerMask groundLayers;


    [Header("Target preview")]

    public Material previewmaterial;
    public Color previewColor = new Color(1f, 0.4f, 0f, 0.9f);
    public float previewLineWidth = 0.15f;

    private float cooldownRemaining;
    private bool fireBallselected;

    private LineRenderer previewRing;



    void Start()
    {
        GameObject previewObject = new GameObject("fireball Target Preview");
        previewRing = previewObject.AddComponent<LineRenderer>();
        previewRing.sharedMaterial = previewmaterial;
        previewRing.useWorldSpace = false;
        previewRing.loop = true;
        previewRing.positionCount = 64;
        previewRing.widthMultiplier = previewLineWidth;
        previewRing.startColor = previewColor;
        previewRing.endColor = previewColor;
        previewRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        previewRing.receiveShadows = false;
        previewRing.enabled = false;
        RefreshUI();
    }


    public void SelectFireBall()
    {
        if (cooldownRemaining > 0f || fireballPrefab == null)
        {
            return;
        }

        //Read the actual damage radius from the projectile.

        FireballMovement fireball = fireballPrefab.GetComponentInChildren<FireballMovement>(true);

        if (fireball == null)
        {
            Debug.LogError("Fireball prefab needs fireballMovement.", this);
            return;
        }

        if (previewRing == null)
        {
            return;
        }

        for (int i = 0; i < previewRing.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / previewRing.positionCount;

            previewRing.SetPosition(i, new Vector3(Mathf.Cos(angle) * fireball.hitRadius, 0.08f, Mathf.Sin(angle) * fireball.hitRadius));
        }

        fireBallselected = true;
        RefreshUI();
    }


    void Update()
    {

        cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);

        RefreshUI();

        if (previewRing != null)
        {
            previewRing.enabled = false;
        }

        if (!fireBallselected || cooldownRemaining > 0f || Mouse.current == null || Time.timeScale == 0f)
        {
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            fireBallselected = false;
            RefreshUI();
            return;
        }

        // hide the preview and prevent casting over UI

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera gameCamera = Camera.main;

        if (gameCamera == null || fireballPrefab == null || previewRing == null)
        {
            return;
        }

        Ray ray = gameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        //Update every frame so the ring follows the cursor
        previewRing.transform.position = hit.point;
        previewRing.enabled = true;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        Vector3 spawnPosition = hit.point + Vector3.up * spawnHeight;
        GameObject projectile = Instantiate(fireballPrefab,spawnPosition,Quaternion.identity);
        FireballMovement movement = projectile.GetComponentInChildren<FireballMovement>(true);
        movement.SetTarget(hit.point,projectile.transform);
        fireBallselected = false;
        previewRing.enabled = false;
        cooldownRemaining = cooldownDuration;
        RefreshUI();
    }



    private void RefreshUI()
    {
        bool ready = cooldownRemaining <= 0f;

        if (fireBallButton != null)
        {
            fireBallButton.interactable = ready;
        }

        if (coolDownText != null)
        {
            coolDownText.text = !ready ? $"{Mathf.CeilToInt(cooldownRemaining)}s" : fireBallselected ? "Click map" : "fireball";
        }
    }

    private void OnDisable()
    {
        fireBallselected = false;

        if (previewRing != null)
        {
            previewRing.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (previewRing != null)
        {
            Destroy(previewRing.gameObject);
        }
    }
}
