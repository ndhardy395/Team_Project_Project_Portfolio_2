using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoUI : MonoBehaviour
{
    private const float SelectionTrailDuration = 0.35f;
    private static readonly Color SelectionColor = new Color(1f, 0.84f, 0.12f);

    public GameObject infoPanel;
    public TMP_Text enemyNameText;
    public TMP_Text descriptionText;
    public TMP_Text hpText;
    public Slider healthSlider;

    [SerializeField] private bool showInfoPanel = true;

    private EnemyHealthDisplay selectedEnemy;
    private Renderer[] selectedRenderers;
    private Camera selectionCamera;
    private Canvas hudCanvas;
    private RectTransform hudRect;
    private RectTransform selectionMarker;
    private RawImage selectionTrail;
    private float selectionTrailStartedAt;

    public EnemyHealthDisplay SelectedEnemy => selectedEnemy != null &&
        selectedEnemy.isActiveAndEnabled && !selectedEnemy.IsDead ? selectedEnemy : null;

    private void Awake()
    {
        hudCanvas = GetComponentInParent<Canvas>();
        if (hudCanvas != null)
            hudRect = (RectTransform)hudCanvas.transform;

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.wholeNumbers = false;
        }
    }

    public void SelectEnemy(EnemyHealthDisplay enemy)
    {
        SelectEnemy(enemy, Camera.main);
    }

    public void SelectEnemy(EnemyHealthDisplay enemy, Camera camera)
    {
        if (enemy != selectedEnemy)
            selectedRenderers = enemy != null ? enemy.GetComponentsInChildren<Renderer>() : null;

        selectedEnemy = enemy;
        selectionCamera = camera;
        selectionTrailStartedAt = Time.unscaledTime;
        RefreshDisplay();
    }

    private void LateUpdate()
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        EnemyHealthDisplay enemy = SelectedEnemy;
        UpdateSelectionMarker(enemy);

        if (infoPanel != null)
            infoPanel.SetActive(showInfoPanel && enemy != null);

        if (!showInfoPanel || enemy == null)
            return;

        if (enemyNameText != null)
            enemyNameText.text = enemy.enemyName;

        if (descriptionText != null)
            descriptionText.text = enemy.discription;

        if (hpText != null)
            hpText.text = $"HP: {Mathf.CeilToInt(enemy.CurrentHealth)} / {Mathf.CeilToInt(enemy.MaxHealth)}";

        if (healthSlider != null)
            healthSlider.value = enemy.MaxHealth > 0f ? enemy.CurrentHealth / enemy.MaxHealth : 0f;
    }
    private void HideSelectionVisuals()
    {
        if (selectionMarker != null)
            selectionMarker.gameObject.SetActive(false);

        if (selectionTrail != null)
            selectionTrail.gameObject.SetActive(false);
    }

    private void UpdateSelectionMarker(EnemyHealthDisplay enemy)
    {
        if (enemy == null || selectionCamera == null || hudRect == null)
        {
            HideSelectionVisuals();
            return;
        }

        Vector3 screenPoint = selectionCamera.WorldToScreenPoint(enemy.transform.position);
        float top = screenPoint.y;
        Bounds? modelBounds = null;

        foreach (Renderer renderer in selectedRenderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy ||
                !(renderer is MeshRenderer || renderer is SkinnedMeshRenderer || renderer is SpriteRenderer))
                continue;

            Bounds bounds = renderer.bounds;
            Bounds combinedBounds = modelBounds ?? bounds;
            combinedBounds.Encapsulate(bounds);
            modelBounds = combinedBounds;

            Vector3 center = selectionCamera.WorldToScreenPoint(bounds.center);
            if (center.z <= 0f)
                continue;

            float screenRadius = 0f;
            for (int axis = 0; axis < 3; axis++)
            {
                Vector3 extent = Vector3.zero;
                extent[axis] = bounds.extents[axis];
                Vector3 edge = selectionCamera.WorldToScreenPoint(bounds.center + extent);
                screenRadius += Mathf.Abs(edge.y - center.y);
            }

            top = Mathf.Max(top, center.y + screenRadius);
        }

        Rect viewport = selectionCamera.pixelRect;
        Vector2 markerPoint = new Vector2(screenPoint.x, Mathf.Min(top + 12f, viewport.yMax - 12f));
        Camera uiCamera = hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : hudCanvas.worldCamera;

        if (screenPoint.z <= 0f || !viewport.Contains(screenPoint) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                hudRect, markerPoint, uiCamera, out Vector2 localPoint))
        {
            HideSelectionVisuals();
            return;
        }

        if (selectionMarker == null)
        {
            selectionMarker = CreateMarkerSquare(
                "Selected Enemy Marker", hudRect, new Color(0.12f, 0.09f, 0.025f), 28f);
            selectionMarker.localRotation = Quaternion.Euler(0f, 0f, 45f);
            CreateMarkerSquare("Yellow Fill", selectionMarker, SelectionColor, 20f);
        }

        selectionMarker.localPosition = localPoint;
        selectionMarker.gameObject.SetActive(true);

        Vector3 targetPoint = modelBounds.HasValue
            ? selectionCamera.WorldToScreenPoint(modelBounds.Value.center)
            : screenPoint;

        UpdateSelectionTrail(localPoint, targetPoint, uiCamera);
    }

    private void UpdateSelectionTrail(Vector2 start, Vector3 targetPoint, Camera uiCamera)
    {
        if (targetPoint.z <= 0f ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                hudRect, targetPoint, uiCamera, out Vector2 target))
        {
            if (selectionTrail != null)
                selectionTrail.gameObject.SetActive(false);

            return;
        }

        float progress = Mathf.Clamp01(
            (Time.unscaledTime - selectionTrailStartedAt) / SelectionTrailDuration);
        Vector2 head = Vector2.Lerp(start, target, progress);
        Vector2 direction = head - start;

        if (selectionTrail == null)
        {
            RectTransform trailRect = CreateMarkerSquare("Selection Trail", hudRect, SelectionColor, 1f);
            trailRect.SetSiblingIndex(selectionMarker.GetSiblingIndex());
            selectionTrail = trailRect.GetComponent<RawImage>();
        }

        RectTransform rect = selectionTrail.rectTransform;
        rect.localPosition = (head + start) * 0.5f;
        rect.localRotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        rect.sizeDelta = new Vector2(direction.magnitude, 6f);
        selectionTrail.gameObject.SetActive(true);
    }

    private RectTransform CreateMarkerSquare(string markerName, RectTransform parent, Color color, float size)
    {
        GameObject square = new GameObject(markerName, typeof(RectTransform), typeof(RawImage));
        RectTransform rect = (RectTransform)square.transform;
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(size, size);

        RawImage image = square.GetComponent<RawImage>();
        image.texture = Texture2D.whiteTexture;
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private void OnDisable()
    {
        HideSelectionVisuals();
    }

    private void OnDestroy()
    {
        if (selectionMarker != null)
            Destroy(selectionMarker.gameObject);

        if (selectionTrail != null)
            Destroy(selectionTrail.gameObject);
    }
}