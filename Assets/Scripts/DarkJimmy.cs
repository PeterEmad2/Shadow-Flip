using UnityEngine;

public class Dark : MonoBehaviour
{
    public PlayerMovement Jimmy;
    private Rigidbody2D rb;
    public float moveSpeed = 6f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (rb == null || Jimmy == null) return;
        rb.linearVelocity = Time.timeScale == 0f ? Vector2.zero : Jimmy.moveInput * moveSpeed;
    }

    void OnDisable()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}
