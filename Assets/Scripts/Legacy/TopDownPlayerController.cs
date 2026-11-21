using UnityEngine;

/// <summary>
/// Smooth 8-directional top-down Rigidbody2D movement controller with automatic
/// directional sprite swapping (0=Down, 1=Up, 2=Left, 3=Right).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;

    [Header("Directional Sprites (0=Down, 1=Up, 2=Left, 3=Right)")]
    public Sprite[] directionalSprites = new Sprite[4];
    public SpriteRenderer spriteRenderer;

    public Vector2 MoveInput { get; private set; }

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            MoveInput = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 raw = new Vector2(x, y);
        MoveInput = raw.sqrMagnitude > 1f ? raw.normalized : raw;

        UpdateFacingSprite();
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        rb.linearVelocity = Time.timeScale == 0f ? Vector2.zero : MoveInput * moveSpeed;
    }

    void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdateFacingSprite()
    {
        if (spriteRenderer == null || directionalSprites == null || directionalSprites.Length < 4) return;
        if (MoveInput.sqrMagnitude < 0.01f) return;

        int index;
        if (Mathf.Abs(MoveInput.y) >= Mathf.Abs(MoveInput.x))
        {
            index = MoveInput.y < 0f ? 0 : 1; // 0=Down, 1=Up
        }
        else
        {
            index = MoveInput.x < 0f ? 2 : 3; // 2=Left, 3=Right
        }

        if (directionalSprites[index] != null)
        {
            spriteRenderer.sprite = directionalSprites[index];
        }
    }
}
