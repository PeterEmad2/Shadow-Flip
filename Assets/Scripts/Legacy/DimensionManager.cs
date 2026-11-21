using System;
using ShadowFlip.Optics;
using ShadowFlip.Puzzle;
using UnityEngine;

/// <summary>
/// Manages switching between the Light and Umbra realms via the F key,
/// updating the player's physics layer and preventing flips into solid obstacles.
/// </summary>
public class DimensionManager : MonoBehaviour
{
    public enum Dimension
    {
        Light,
        Umbra
    }

    public static DimensionManager Instance { get; private set; }
    public static event Action<Dimension> OnDimensionChanged;

    [Header("Controls & References")]
    public KeyCode flipKey = KeyCode.F;
    public Transform playerTransform;
    public float overlapCheckRadius = 0.35f;

    [Header("Physics Layers")]
    public string lightWorldLayerName = "LightWorld";
    public string umbraWorldLayerName = "UmbraWorld";

    public Dimension CurrentDimension { get; private set; } = Dimension.Light;
    public int LightWorldLayer { get; private set; } = -1;
    public int UmbraWorldLayer { get; private set; } = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        OnDimensionChanged = null;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LightWorldLayer = LayerMask.NameToLayer(lightWorldLayerName);
        UmbraWorldLayer = LayerMask.NameToLayer(umbraWorldLayerName);
    }

    void Start()
    {
        if (playerTransform == null)
        {
            TopDownPlayerController controller = FindFirstObjectByType<TopDownPlayerController>();
            if (controller != null)
            {
                playerTransform = controller.transform;
            }
            else
            {
                PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
                if (pm != null) playerTransform = pm.transform;
            }
        }

        if (logic.Instance != null)
        {
            CurrentDimension = logic.Instance.isDarkActive ? Dimension.Umbra : Dimension.Light;
        }

        ApplyPlayerLayer(CurrentDimension);
        OnDimensionChanged?.Invoke(CurrentDimension);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetKeyDown(flipKey))
        {
            TryToggleDimension();
        }
    }

    public bool TryToggleDimension()
    {
        Dimension target = CurrentDimension == Dimension.Light ? Dimension.Umbra : Dimension.Light;
        return TrySetDimension(target);
    }

    public bool TrySetDimension(Dimension target)
    {
        if (CurrentDimension == target) return false;

        Vector2 checkPos = GetPlayerPosition();
        if (WouldOverlapSolidInTargetDimension(checkPos, target))
        {
            PuzzleAudioManager.Instance?.PlayFlipBlocked();
            return false;
        }

        CurrentDimension = target;
        ApplyPlayerLayer(CurrentDimension);

        if (logic.Instance != null)
        {
            logic.Instance.SetWorld(CurrentDimension == Dimension.Umbra);
        }

        OnDimensionChanged?.Invoke(CurrentDimension);
        return true;
    }

    private Vector2 GetPlayerPosition()
    {
        if (playerTransform != null) return playerTransform.position;
        TopDownPlayerController controller = FindFirstObjectByType<TopDownPlayerController>();
        if (controller != null) return controller.transform.position;
        return transform.position;
    }

    private bool WouldOverlapSolidInTargetDimension(Vector2 position, Dimension target)
    {
        int targetLayer = target == Dimension.Light ? LightWorldLayer : UmbraWorldLayer;

        Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, overlapCheckRadius);
        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider2D col = overlaps[i];
            if (col == null || col.isTrigger) continue;
            if (IsPlayerCollider(col)) continue;

            if (targetLayer >= 0 && col.gameObject.layer == targetLayer)
            {
                return true;
            }
        }

        DimensionObject[] dimObjects = FindObjectsByType<DimensionObject>(FindObjectsSortMode.None);
        for (int i = 0; i < dimObjects.Length; i++)
        {
            if (dimObjects[i].WouldBlockPosition(position, overlapCheckRadius, target))
            {
                return true;
            }
        }

        WorldPhaseObject[] phaseObjects = FindObjectsByType<WorldPhaseObject>(FindObjectsSortMode.None);
        bool targetDark = target == Dimension.Umbra;
        for (int i = 0; i < phaseObjects.Length; i++)
        {
            if (phaseObjects[i].WouldBlockPosition(position, overlapCheckRadius, targetDark))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPlayerCollider(Collider2D col)
    {
        if (playerTransform != null && (col.transform == playerTransform || col.transform.IsChildOf(playerTransform)))
        {
            return true;
        }
        return col.GetComponent<TopDownPlayerController>() != null
            || col.GetComponent<Light>() != null
            || col.GetComponent<Dark>() != null;
    }

    private void ApplyPlayerLayer(Dimension dim)
    {
        if (playerTransform == null) return;
        int layer = dim == Dimension.Light ? LightWorldLayer : UmbraWorldLayer;
        if (layer >= 0)
        {
            playerTransform.gameObject.layer = layer;
        }
    }
}
