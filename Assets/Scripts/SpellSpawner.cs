using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
public class SpellSpawner : MonoBehaviour
{
    [Header("Fireball")]
    public GameObject fireballPrefab;
    public Button fireButton;
    public TMP_Text fireCoolDownText;
    public float fireCooldownDuration = 10f;

    [Header("Ice")]
    public GameObject iceballPrefab;
    public Button iceButton;
    public TMP_Text iceCoolDownText;
    public float iceCooldownDuration = 12f;

    [Header("Lightning")]
    public GameObject lightningPrefab;
    public Button lightningButton;
    public TMP_Text lightningcoolDownText;
    public float lightningCooldownDuration = 15f;


    [Header("Targeting")]
    public float spawnHeight = 10f;
    public LayerMask groundLayers;

    [Header("Target preview")]
    public Material firePreviewMaterial;
    public Color firePreviewColor = new Color(1f, 0.4f, 0f, 0.9f);
    public Color icePreviewColor = new Color(0f, 0.8f, 1f, 0.9f);
    public Color LightningPreviewColor = new Color(1f, 0.9f, 0f, 0.9f);

    public float previewLineWidth = 0.15f;

    private enum SelectedSpell
    {
        None, Fireball, Ice, Lightning
    }

    private SelectedSpell selectedSpell;
    private float fireCoolDownRemaning;
    private float iceCoolDownRemaning;
    private float lightningCoolDownRemaning;
    private LineRenderer previewRing;

    void Start()
    {
        GameObject previewObject = new GameObject("Spell Target Preview");
        previewRing = previewObject.AddComponent<LineRenderer>();

        previewRing.sharedMaterial = firePreviewMaterial;
        previewRing.useWorldSpace = false;
        previewRing.loop = true;
        previewRing.positionCount = 64;
        previewRing.widthMultiplier = previewLineWidth;
        previewRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        previewRing.receiveShadows = false;
        previewRing.enabled = false;

        RefreshUI();
    }
    private void RestGameObject()
    {
        
        GameObject previewObject = new GameObject("Spell Target Preview");
        previewRing = previewObject.AddComponent<LineRenderer>();
        
        previewRing.sharedMaterial = firePreviewMaterial;
        previewRing.useWorldSpace = false;
        previewRing.loop = true;
        previewRing.positionCount = 64;
        previewRing.widthMultiplier = previewLineWidth;
        previewRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        previewRing.receiveShadows = false;
        previewRing.enabled = false;
        Debug.Log("activ");
        
        RefreshUI();
    }
    public void SelectFireBall()
    {
        
        if (fireCoolDownRemaning > 0f || fireballPrefab == null || previewRing == null)
        {

            return;
        }

        FireballMovement spell = fireballPrefab.GetComponentInChildren<FireballMovement>(true);

        if(spell == null)
        {
            Debug.LogError("Fireball prefab need FireBallmovement.",this);
            return;
        }
        
        SelectSpell(SelectedSpell.Fireball, spell.hitRadius, firePreviewColor);
    }

    public void SelectIceBall()
    {
        if (iceCoolDownRemaning > 0f || iceballPrefab == null || previewRing == null)
        {
            return;
        }

        IceBallDrop spell = iceballPrefab.GetComponentInChildren<IceBallDrop>(true);

        if(spell == null)
        {
            Debug.LogError("Ice prefab need IceBallDrop.", this);
            return;
        }

        SelectSpell(SelectedSpell.Ice,spell.hitRadius, icePreviewColor);
    }

    public void SelcetedLightning()
    {
        if (lightningCoolDownRemaning > 0f || lightningPrefab == null || previewRing == null)
        {
            return;
        }
        LightningSpell spell = lightningPrefab.GetComponentInChildren<LightningSpell>(true);

        if(spell == null)
        {
            Debug.LogError("Lightning prefab needs Lightning Spell.", this);
            return;
        }

        SelectSpell(SelectedSpell.Lightning, spell.hitRadius, LightningPreviewColor);
    }

    private void SelectSpell(SelectedSpell spell, float raduis, Color color)
    {
        selectedSpell = spell;

        previewRing.enabled = false;
        previewRing.startColor = color;
        previewRing.endColor = color;
       
        for(int i = 0; i < previewRing.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / previewRing.positionCount;

            previewRing.SetPosition(i,new Vector3(Mathf.Cos(angle) * raduis,0.008f,Mathf.Sin(angle) * raduis));
        }

        RefreshUI();
    }

    void Update()
    {
        if (previewRing == null)
        {
            RestGameObject();
        }
        fireCoolDownRemaning = Mathf.Max(0f, fireCoolDownRemaning - Time.deltaTime);
        iceCoolDownRemaning = Mathf.Max(0f, iceCoolDownRemaning - Time.deltaTime);
        lightningCoolDownRemaning = Mathf.Max(0f, lightningCoolDownRemaning - Time.deltaTime);

        RefreshUI();
        
        if(previewRing != null)
        {
            previewRing.enabled = false;
        }
        

        if (selectedSpell == SelectedSpell.None || Mouse.current == null || Time.timeScale == 0f)
        {
            return;
        }

        if(Mouse.current.rightButton.wasPressedThisFrame)
        {
            selectedSpell = SelectedSpell.None;
            RefreshUI();
            return;
        }

        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera gameCamera = Camera.main;

        if(gameCamera == null || previewRing == null)
        {
            return;
        }

        Ray ray = gameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity,groundLayers,QueryTriggerInteraction.Ignore))
        {
            return;
        }

        previewRing.transform.position = hit.point;
        previewRing.enabled = true;

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            CastSpell(hit.point);
        }
    }

    private void CastSpell(Vector3 targetPosition)
    {
        GameObject projectile;
        
        switch (selectedSpell)
        {
            case SelectedSpell.Fireball:
                {
                    if (fireCoolDownRemaning > 0f || fireballPrefab == null)
                    {
                        return;
                    }

                    projectile = Instantiate(fireballPrefab, targetPosition + Vector3.up * spawnHeight, Quaternion.identity);
                    FireballMovement spell = projectile.GetComponentInChildren<FireballMovement>(true);

                    if (spell == null)
                    {
                        Destroy(projectile);
                        return;
                    }

                    spell.SetTarget(targetPosition, projectile.transform);

                    fireCoolDownRemaning = fireCooldownDuration;
                    break;
                }

            case SelectedSpell.Ice:

                {
                    if (iceCoolDownRemaning > 0f || iceballPrefab == null)
                    {
                        return;
                    }

                    projectile = Instantiate(iceballPrefab, targetPosition + Vector3.up * spawnHeight, Quaternion.identity);
                    IceBallDrop spell = projectile.GetComponentInChildren<IceBallDrop>(true);

                    if (spell == null)
                    {
                        Destroy(projectile);
                        return;
                    }

                    spell.SetTarget(targetPosition, projectile.transform);

                    iceCoolDownRemaning = iceCooldownDuration;
                    break;
                }

            case SelectedSpell.Lightning:

                {
                    if (lightningCoolDownRemaning > 0f || lightningPrefab == null)
                    {
                        return;
                    }

                    projectile = Instantiate(lightningPrefab, targetPosition, Quaternion.identity);
                    LightningSpell spell = projectile.GetComponentInChildren<LightningSpell>(true);

                    if (spell == null)
                    {
                        Destroy(projectile);
                        return;
                    }

                    spell.SetTarget(targetPosition, projectile.transform);

                    lightningCoolDownRemaning = lightningCooldownDuration;
                    break;
                }
            default:
                return;
        }
        selectedSpell = SelectedSpell.None;
        previewRing.enabled = false;
        RefreshUI();
    }

    private void RefreshUI()
    {
      
        UpdateSpellUI(fireButton, fireCoolDownText, fireballPrefab, fireCoolDownRemaning, SelectedSpell.Fireball, "Fireball");
        UpdateSpellUI(iceButton, iceCoolDownText, iceballPrefab, iceCoolDownRemaning, SelectedSpell.Ice, "Ice");
        UpdateSpellUI(lightningButton, lightningcoolDownText, lightningPrefab, lightningCoolDownRemaning, SelectedSpell.Lightning, "Lightning");
    }

    private void UpdateSpellUI(Button button, TMP_Text label, GameObject prefab, float remaining, SelectedSpell spell, string spellName)
    {
        bool ready = remaining <= 0f;

        if(button != null)
        {
            button.interactable = ready && prefab != null;
        }

        if(label != null)
        {
            label.text = !ready ? $"{Mathf.CeilToInt(remaining)}s" : selectedSpell == spell ? "Click map" : spellName;
        }

      
    }

    private void OnDisable()
    {
        selectedSpell = SelectedSpell.None;

        if(previewRing != null)
        {
            previewRing.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if(previewRing != null)
        {
            Destroy(previewRing.gameObject);
        }
    }
}
