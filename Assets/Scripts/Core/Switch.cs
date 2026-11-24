using ShadowFlip.Optics;
using ShadowFlip.Puzzle;
using UnityEngine;

public class GameObjectToggle : MonoBehaviour
{
    public GameObject darkObject;
    public GameObject lightObject;
    public logic logic;

    [Header("Flip Settings")]
    public KeyCode flipKey = KeyCode.F;
    public float flipCooldown = 0.15f;
    public LayerMask blockingMask;
    public float safetyCheckRadius = 0.3f;

    private float lastFlipTime = -10f;
    private Rigidbody2D lightRb;
    private Rigidbody2D darkRb;

    void Start()
    {
        if (logic == null) logic = logic.Instance;
        if (lightObject != null) lightRb = lightObject.GetComponent<Rigidbody2D>();
        if (darkObject != null) darkRb = darkObject.GetComponent<Rigidbody2D>();

        bool darkActive = logic != null && logic.isDarkActive;
        if (darkObject != null) darkObject.SetActive(darkActive);
        if (lightObject != null) lightObject.SetActive(!darkActive);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetKeyDown(flipKey) && Time.unscaledTime - lastFlipTime >= flipCooldown)
        {
            TryFlip();
        }
    }

    public bool TryFlip()
    {
        if (logic == null || darkObject == null || lightObject == null) return false;

        GameObject currentObj = logic.isDarkActive ? darkObject : lightObject;
        GameObject targetObj = logic.isDarkActive ? lightObject : darkObject;
        bool nextDark = !logic.isDarkActive;

        // Prevent flipping if a solid phased object in the target world would overlap the player.
        var phasedObjects = WorldPhaseObject.ActiveObjects;
        for (int i = 0; i < phasedObjects.Count; i++)
        {
            WorldPhaseObject obj = phasedObjects[i];
            if (obj != null && obj.WouldBlockPosition(currentObj.transform.position, safetyCheckRadius, nextDark))
            {
                PuzzleAudioManager.Instance?.PlayFlipBlocked();
                return false;
            }
        }

        // Prevent flipping if a solid obstacle on blockingMask overlaps the player.
        if (blockingMask.value != 0)
        {
            Collider2D hit = Physics2D.OverlapCircle(currentObj.transform.position, safetyCheckRadius, blockingMask);
            if (hit != null && !hit.isTrigger && hit.gameObject != currentObj && hit.gameObject != targetObj)
            {
                PuzzleAudioManager.Instance?.PlayFlipBlocked();
                return false;
            }
        }

        lastFlipTime = Time.unscaledTime;

        targetObj.transform.position = currentObj.transform.position;

        Rigidbody2D currentRb = nextDark ? lightRb : darkRb;
        if (currentRb != null) currentRb.linearVelocity = Vector2.zero;

        darkObject.SetActive(nextDark);
        lightObject.SetActive(!nextDark);
        logic.SetWorld(nextDark);
        return true;
    }
}
