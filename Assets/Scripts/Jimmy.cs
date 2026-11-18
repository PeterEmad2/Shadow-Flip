using ShadowFlip.Optics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public Vector2 moveInput;
    public bool DarkisMoving = false;
    public bool LightisMoving = false;
    public bool isMoving = false;
    public SpriteRenderer LightspriteRenderer;
    public SpriteRenderer DarkspriteRenderer;
    public Sprite LightspriteUp;
    public Sprite LightspriteDown;
    public Sprite LightspriteLeft;
    public Sprite LightspriteRight;

    public Sprite DarkspriteUp;
    public Sprite DarkspriteDown;
    public Sprite DarkspriteLeft;
    public Sprite DarkspriteRight;
    public logic logic;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRadius = 1.6f;

    public IInteractable NearbyInteractable { get; private set; }

    void Start()
    {
        if (logic == null) logic = logic.Instance;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            moveInput = Vector2.zero;
            isMoving = false;
            DarkisMoving = false;
            LightisMoving = false;
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        isMoving = moveInput.sqrMagnitude > 0.01f;
        bool darkActive = logic != null && logic.isDarkActive;
        DarkisMoving = darkActive && isMoving;
        LightisMoving = !darkActive && isMoving;

        UpdateDirectionalSprites();
        UpdateInteraction();
    }

    public Transform GetActiveFormTransform()
    {
        bool darkActive = logic != null && logic.isDarkActive;
        if (darkActive && DarkspriteRenderer != null) return DarkspriteRenderer.transform;
        if (!darkActive && LightspriteRenderer != null) return LightspriteRenderer.transform;
        return transform;
    }

    private void UpdateDirectionalSprites()
    {
        if (moveInput.y > 0)
        {
            if (LightspriteRenderer != null && LightspriteUp != null) LightspriteRenderer.sprite = LightspriteUp;
            if (DarkspriteRenderer != null && DarkspriteUp != null) DarkspriteRenderer.sprite = DarkspriteUp;
        }
        else if (moveInput.y < 0)
        {
            if (LightspriteRenderer != null && LightspriteDown != null) LightspriteRenderer.sprite = LightspriteDown;
            if (DarkspriteRenderer != null && DarkspriteDown != null) DarkspriteRenderer.sprite = DarkspriteDown;
        }
        else if (moveInput.x < 0)
        {
            if (LightspriteRenderer != null && LightspriteLeft != null) LightspriteRenderer.sprite = LightspriteLeft;
            if (DarkspriteRenderer != null && DarkspriteLeft != null) DarkspriteRenderer.sprite = DarkspriteLeft;
        }
        else if (moveInput.x > 0)
        {
            if (LightspriteRenderer != null && LightspriteRight != null) LightspriteRenderer.sprite = LightspriteRight;
            if (DarkspriteRenderer != null && DarkspriteRight != null) DarkspriteRenderer.sprite = DarkspriteRight;
        }
    }

    private void UpdateInteraction()
    {
        Transform origin = GetActiveFormTransform();
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin.position, interactRadius);

        IInteractable closest = null;
        float bestDistSq = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.isTrigger && hit.GetComponent<IInteractable>() == null) continue;
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null) interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float distSq = ((Vector2)hit.transform.position - (Vector2)origin.position).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                closest = interactable;
            }
        }

        NearbyInteractable = closest;

        if (NearbyInteractable != null && Input.GetKeyDown(interactKey))
        {
            NearbyInteractable.Interact(this);
        }
    }
}
