using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dual-realm optical beam emitter using Physics2D.Raycast and LineRenderer with up to 10 mirror bounces.
/// Light mode fires a gold/cyan Light Laser blocked by LightWorld obstacles;
/// Umbra mode fires a dark violet Shadow Beam that passes through LightWorld obstacles,
/// stops at UmbraWorld obstacles, and activates Umbra BeamReceiver crystals.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class ShadowBeamEmitter : MonoBehaviour
{
    [Header("Ray Settings")]
    public Vector2 emitDirection = Vector2.right;
    public float maxDistance = 30f;
    [Range(1, 10)]
    public int maxBounces = 10;
    public float beamWidth = 0.16f;
    public string mirrorTag = "Mirror";
    public LayerMask hitMask = ~0;

    [Header("Light Laser Style")]
    public Color lightStartColor = new Color(1f, 0.92f, 0.32f, 1f);
    public Color lightEndColor = new Color(0.28f, 0.94f, 1f, 1f);

    [Header("Umbra Shadow Beam Style")]
    public Color umbraStartColor = new Color(0.58f, 0.16f, 0.92f, 1f);
    public Color umbraEndColor = new Color(0.08f, 0.03f, 0.14f, 1f);

    private LineRenderer lineRenderer;
    private readonly List<Vector3> points = new List<Vector3>(16);
    private int lightWorldLayer = -1;
    private int umbraWorldLayer = -1;
    private DimensionManager.Dimension activeDimension = DimensionManager.Dimension.Light;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
        lineRenderer.numCapVertices = 0;
        lineRenderer.numCornerVertices = 0;

        if (lineRenderer.sharedMaterial == null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                lineRenderer.material = new Material(spriteShader);
            }
        }

        lightWorldLayer = LayerMask.NameToLayer("LightWorld");
        umbraWorldLayer = LayerMask.NameToLayer("UmbraWorld");
    }

    void OnEnable()
    {
        DimensionManager.OnDimensionChanged += HandleDimensionChanged;
        SyncCurrentDimension();
    }

    void OnDisable()
    {
        DimensionManager.OnDimensionChanged -= HandleDimensionChanged;
    }

    void LateUpdate()
    {
        SyncCurrentDimension();
        UpdateBeamStyle();
        TraceBeam();
    }

    private void HandleDimensionChanged(DimensionManager.Dimension newDim)
    {
        activeDimension = newDim;
        UpdateBeamStyle();
    }

    private void SyncCurrentDimension()
    {
        if (DimensionManager.Instance != null)
        {
            activeDimension = DimensionManager.Instance.CurrentDimension;
        }
        else if (logic.Instance != null)
        {
            activeDimension = logic.Instance.isDarkActive
                ? DimensionManager.Dimension.Umbra
                : DimensionManager.Dimension.Light;
        }
    }

    private void UpdateBeamStyle()
    {
        if (lineRenderer == null) return;
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;

        if (activeDimension == DimensionManager.Dimension.Light)
        {
            lineRenderer.startColor = lightStartColor;
            lineRenderer.endColor = lightEndColor;
        }
        else
        {
            lineRenderer.startColor = umbraStartColor;
            lineRenderer.endColor = umbraEndColor;
        }
    }

    private void TraceBeam()
    {
        points.Clear();

        Vector2 origin = transform.position;
        Vector2 direction = ((Vector2)transform.TransformDirection(emitDirection.normalized)).normalized;
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;

        points.Add(new Vector3(origin.x, origin.y, 0f));

        Collider2D ignoreCollider = null;
        int remainingBounces = Mathf.Clamp(maxBounces, 1, 10);

        for (int step = 0; step <= remainingBounces; step++)
        {
            RaycastHit2D hit = FindNextBlockingHit(origin, direction, ignoreCollider);
            if (hit.collider == null)
            {
                Vector2 end = origin + direction * maxDistance;
                points.Add(new Vector3(end.x, end.y, 0f));
                break;
            }

            points.Add(new Vector3(hit.point.x, hit.point.y, 0f));

            BeamReceiver receiver = hit.collider.GetComponent<BeamReceiver>();
            if (receiver == null) receiver = hit.collider.GetComponentInParent<BeamReceiver>();
            if (receiver != null)
            {
                receiver.NotifyBeamHit(activeDimension);
                break;
            }

            if (IsMirrorCollider(hit.collider) && step < remainingBounces)
            {
                direction = Vector2.Reflect(direction, hit.normal).normalized;
                origin = hit.point + direction * 0.02f;
                ignoreCollider = hit.collider;
                continue;
            }

            break;
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    private RaycastHit2D FindNextBlockingHit(Vector2 origin, Vector2 direction, Collider2D ignoreCollider)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance, hitMask);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D h = hits[i];
            Collider2D col = h.collider;
            if (col == null || col == ignoreCollider) continue;

            // Player bodies do not block beams.
            if (col.GetComponent<TopDownPlayerController>() != null
                || col.GetComponent<Light>() != null
                || col.GetComponent<Dark>() != null)
            {
                continue;
            }

            BeamReceiver receiver = col.GetComponent<BeamReceiver>();
            if (receiver == null) receiver = col.GetComponentInParent<BeamReceiver>();
            if (col.isTrigger && receiver == null && !IsMirrorCollider(col)) continue;

            if (!BlocksBeamInCurrentDimension(col)) continue;

            return h;
        }

        return default;
    }

    private bool BlocksBeamInCurrentDimension(Collider2D col)
    {
        DimensionObject dimObj = col.GetComponent<DimensionObject>();
        if (dimObj == null) dimObj = col.GetComponentInParent<DimensionObject>();
        if (dimObj != null && !dimObj.IsActiveIn(activeDimension))
        {
            return false;
        }

        int layer = col.gameObject.layer;
        if (activeDimension == DimensionManager.Dimension.Umbra)
        {
            // Shadow Beam passes through LightWorld obstacles and stops on UmbraWorld obstacles.
            if (lightWorldLayer >= 0 && layer == lightWorldLayer) return false;
        }
        else
        {
            // Light Laser passes through UmbraWorld obstacles and stops on LightWorld obstacles.
            if (umbraWorldLayer >= 0 && layer == umbraWorldLayer) return false;
        }

        return true;
    }

    private bool IsMirrorCollider(Collider2D col)
    {
        if (!string.IsNullOrEmpty(mirrorTag) && col.gameObject.tag == mirrorTag)
        {
            return true;
        }
        return col.GetComponent<ShadowFlip.Optics.OpticalMirror>() != null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = lightStartColor;
        Vector3 dir = transform.TransformDirection(emitDirection.normalized);
        Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);
    }
}
