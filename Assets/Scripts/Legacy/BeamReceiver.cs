using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Beam receiver crystal activated when struck by a ShadowBeamEmitter in the matching dimension.
/// </summary>
public class BeamReceiver : MonoBehaviour
{
    [Header("Activation Rules")]
    public DimensionManager.Dimension requiredDimension = DimensionManager.Dimension.Umbra;
    public bool acceptAnyDimension = false;

    [Header("Visuals")]
    public SpriteRenderer crystalRenderer;
    public Color inactiveColor = new Color(0.55f, 0.55f, 0.62f, 1f);
    public Color lightActiveColor = new Color(1f, 0.90f, 0.35f, 1f);
    public Color umbraActiveColor = new Color(0.76f, 0.36f, 1f, 1f);

    [Header("Events")]
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    public bool IsActivated { get; private set; }

    private int lastHitFrame = -100;
    private DimensionManager.Dimension lastHitDimension;

    void Awake()
    {
        if (crystalRenderer == null) crystalRenderer = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    void Update()
    {
        bool hitThisFrame = (Time.frameCount - lastHitFrame) <= 1;
        if (!hitThisFrame && IsActivated)
        {
            SetActivated(false);
        }
    }

    public void NotifyBeamHit(DimensionManager.Dimension beamDimension)
    {
        if (!acceptAnyDimension && beamDimension != requiredDimension) return;

        lastHitFrame = Time.frameCount;
        lastHitDimension = beamDimension;
        if (!IsActivated)
        {
            SetActivated(true);
        }
    }

    private void SetActivated(bool active)
    {
        if (IsActivated == active) return;
        IsActivated = active;
        UpdateVisuals();

        if (IsActivated) onActivated?.Invoke();
        else onDeactivated?.Invoke();
    }

    private void UpdateVisuals()
    {
        if (crystalRenderer == null) return;
        if (!IsActivated)
        {
            crystalRenderer.color = inactiveColor;
            return;
        }

        crystalRenderer.color = lastHitDimension == DimensionManager.Dimension.Umbra
            ? umbraActiveColor
            : lightActiveColor;
    }
}
