using UnityEngine;

/// <summary>
/// Configures an object to exist in LightOnly, UmbraOnly, or Both dimensions.
/// UmbraOnly entities hide completely (alpha = 0) in Light; inactive walls/props ghost at 0.2f alpha.
/// </summary>
public class DimensionObject : MonoBehaviour
{
    public enum ObjectDimension
    {
        LightOnly,
        UmbraOnly,
        Both
    }

    public ObjectDimension objectDimension = ObjectDimension.Both;

    [Tooltip("When enabled (or on UmbraOnly entities with a Rigidbody2D), sets alpha = 0 instead of 0.2f ghosting.")]
    public bool hideCompletelyWhenInactive = false;

    [Range(0f, 1f)]
    public float inactiveAlpha = 0.2f;

    public bool IsActiveInCurrentDimension { get; private set; } = true;

    private Collider2D[] colliders;
    private SpriteRenderer[] renderers;
    private Color[] baseColors;
    private bool autoHideEntity;

    void Awake()
    {
        colliders = GetComponentsInChildren<Collider2D>(true);
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            baseColors[i] = renderers[i].color;
        }

        autoHideEntity = hideCompletelyWhenInactive
            || (objectDimension == ObjectDimension.UmbraOnly && GetComponent<Rigidbody2D>() != null);
    }

    void OnEnable()
    {
        DimensionManager.OnDimensionChanged += HandleDimensionChanged;
        if (logic.Instance != null)
        {
            logic.Instance.OnWorldFlipped += HandleLegacyWorldFlipped;
        }

        DimensionManager.Dimension current = DimensionManager.Instance != null
            ? DimensionManager.Instance.CurrentDimension
            : (logic.Instance != null && logic.Instance.isDarkActive
                ? DimensionManager.Dimension.Umbra
                : DimensionManager.Dimension.Light);

        HandleDimensionChanged(current);
    }

    void OnDisable()
    {
        DimensionManager.OnDimensionChanged -= HandleDimensionChanged;
        if (logic.Instance != null)
        {
            logic.Instance.OnWorldFlipped -= HandleLegacyWorldFlipped;
        }
    }

    public bool IsActiveIn(DimensionManager.Dimension dimension)
    {
        return objectDimension == ObjectDimension.Both
            || (objectDimension == ObjectDimension.LightOnly && dimension == DimensionManager.Dimension.Light)
            || (objectDimension == ObjectDimension.UmbraOnly && dimension == DimensionManager.Dimension.Umbra);
    }

    public bool WouldBlockPosition(Vector2 worldPos, float radius, DimensionManager.Dimension targetDimension)
    {
        if (!IsActiveIn(targetDimension)) return false;

        bool hasSolidCollider = false;
        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && !colliders[i].isTrigger)
                {
                    hasSolidCollider = true;
                    break;
                }
            }
        }
        if (!hasSolidCollider || renderers == null) return false;

        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer sr = renderers[i];
            if (sr == null) continue;
            Bounds expanded = sr.bounds;
            expanded.Expand(radius * 2f);
            if (expanded.Contains(new Vector3(worldPos.x, worldPos.y, expanded.center.z)))
            {
                return true;
            }
        }

        return false;
    }

    private void HandleLegacyWorldFlipped(bool isDarkActive)
    {
        HandleDimensionChanged(isDarkActive ? DimensionManager.Dimension.Umbra : DimensionManager.Dimension.Light);
    }

    private void HandleDimensionChanged(DimensionManager.Dimension newDimension)
    {
        IsActiveInCurrentDimension = IsActiveIn(newDimension);

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = IsActiveInCurrentDimension;
                }
            }
        }

        bool hideCompletely = autoHideEntity || hideCompletelyWhenInactive || objectDimension == ObjectDimension.UmbraOnly && GetComponent<Rigidbody2D>() != null;
        float targetAlphaMultiplier = IsActiveInCurrentDimension ? 1f : (hideCompletely ? 0f : inactiveAlpha);

        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                if (sr == null) continue;

                Color c = baseColors[i];
                c.a = baseColors[i].a * targetAlphaMultiplier;
                sr.color = c;
                sr.enabled = IsActiveInCurrentDimension || !hideCompletely;
            }
        }
    }
}
