using ShadowFlip.Optics;
using ShadowFlip.Puzzle;
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
    public bool IsDarkModeActive => logic != null && logic.isDarkActive;
    public float CurrentSpeedMultiplier => Time.time <= moldSlowUntil ? 0.42f : 1f;

    private int facingIndex = 0; // 0=Down, 1=Up, 2=Left, 3=Right
    private float walkAnimTimer = 0f;
    private float moldSlowUntil = -1f;
    private readonly Sprite[,] lightWalk = new Sprite[4, 2];
    private readonly Sprite[,] darkWalk = new Sprite[4, 2];
    private readonly Collider2D[] interactBuffer = new Collider2D[16];

    public void ApplyMoldSlow(float duration = 0.15f)
    {
        moldSlowUntil = Mathf.Max(moldSlowUntil, Time.time + duration);
    }

    void Start()
    {
        if (logic == null) logic = logic.Instance;
        LoadWalkSprites();
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

        if (isMoving)
        {
            walkAnimTimer += Time.deltaTime;
            if (Mathf.Abs(moveInput.y) >= Mathf.Abs(moveInput.x))
            {
                facingIndex = moveInput.y >= 0f ? 1 : 0;
            }
            else
            {
                facingIndex = moveInput.x < 0f ? 2 : 3;
            }
        }
        else
        {
            walkAnimTimer = 0f;
        }

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

    private void LoadWalkSprites()
    {
        string[] dirs = { "down", "up", "left", "right" };
        for (int d = 0; d < 4; d++)
        {
            lightWalk[d, 0] = Resources.Load<Sprite>($"Sprites/JimmyWalk/light_{dirs[d]}_walk1");
            lightWalk[d, 1] = Resources.Load<Sprite>($"Sprites/JimmyWalk/light_{dirs[d]}_walk2");
            darkWalk[d, 0] = Resources.Load<Sprite>($"Sprites/JimmyWalk/dark_{dirs[d]}_walk1");
            darkWalk[d, 1] = Resources.Load<Sprite>($"Sprites/JimmyWalk/dark_{dirs[d]}_walk2");
        }
    }

    private void UpdateDirectionalSprites()
    {
        Sprite lightIdle = GetLightIdle(facingIndex);
        Sprite darkIdle = GetDarkIdle(facingIndex);

        Sprite chosenLight = lightIdle;
        Sprite chosenDark = darkIdle;

        if (isMoving)
        {
            int step = Mathf.FloorToInt(walkAnimTimer * 8f) % 4; // [Idle, Walk1, Idle, Walk2]
            if (step == 1)
            {
                if (lightWalk[facingIndex, 0] != null) chosenLight = lightWalk[facingIndex, 0];
                if (darkWalk[facingIndex, 0] != null) chosenDark = darkWalk[facingIndex, 0];
            }
            else if (step == 3)
            {
                if (lightWalk[facingIndex, 1] != null) chosenLight = lightWalk[facingIndex, 1];
                if (darkWalk[facingIndex, 1] != null) chosenDark = darkWalk[facingIndex, 1];
            }
        }

        if (LightspriteRenderer != null && chosenLight != null) LightspriteRenderer.sprite = chosenLight;
        if (DarkspriteRenderer != null && chosenDark != null) DarkspriteRenderer.sprite = chosenDark;
    }

    private Sprite GetLightIdle(int dir)
    {
        switch (dir)
        {
            case 1: return LightspriteUp;
            case 2: return LightspriteLeft;
            case 3: return LightspriteRight;
            default: return LightspriteDown;
        }
    }

    private Sprite GetDarkIdle(int dir)
    {
        switch (dir)
        {
            case 1: return DarkspriteUp;
            case 2: return DarkspriteLeft;
            case 3: return DarkspriteRight;
            default: return DarkspriteDown;
        }
    }

    private void UpdateInteraction()
    {
        Transform origin = GetActiveFormTransform();
        int hitCount = Physics2D.OverlapCircle(origin.position, interactRadius, ContactFilter2D.noFilter, interactBuffer);

        IInteractable closest = null;
        float bestDistSq = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = interactBuffer[i];
            if (hit == null) continue;

            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (hit.isTrigger && interactable == null) continue;
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
            if (IsInteractableAllowedInCurrentRealm(NearbyInteractable, out _))
            {
                NearbyInteractable.Interact(this);
            }
            else
            {
                PuzzleAudioManager.Instance?.PlayFlipBlocked();
            }
        }
    }

    public bool IsInteractableAllowedInCurrentRealm(IInteractable target, out string lockMessage)
    {
        lockMessage = string.Empty;
        if (target == null) return false;

        if (target is LabNPCController)
        {
            return true;
        }

        if (target is OpticalMirror mirror)
        {
            if (mirror.requireShadowWorldToRotate)
            {
                if (!IsDarkModeActive)
                {
                    lockMessage = "Umbra Mode Required to Rotate ([F] Flip)";
                    return false;
                }
                return true;
            }

            if (IsDarkModeActive)
            {
                lockMessage = "Light Mode Required to Interact ([F] Flip)";
                return false;
            }
            return true;
        }

        if (target is PuzzleLever lever)
        {
            if (lever.usableInWorld == WorldAffinity.ShadowOnly)
            {
                if (!IsDarkModeActive)
                {
                    lockMessage = "Umbra Mode Required to Pull ([F] Flip)";
                    return false;
                }
                return true;
            }

            if (IsDarkModeActive)
            {
                lockMessage = "Light Mode Required to Interact ([F] Flip)";
                return false;
            }
            return true;
        }

        if (target is BlackHoleLens bh)
        {
            if (bh.activeInWorld == WorldAffinity.ShadowOnly)
            {
                if (!IsDarkModeActive)
                {
                    lockMessage = "Umbra Mode Required to Tune ([F] Flip)";
                    return false;
                }
                return true;
            }

            if (IsDarkModeActive)
            {
                lockMessage = "Light Mode Required to Interact ([F] Flip)";
                return false;
            }
            return true;
        }

        if (target is MonoBehaviour mb)
        {
            WorldPhaseObject phase = mb.GetComponent<WorldPhaseObject>();
            if (phase != null && phase.affinity == WorldAffinity.ShadowOnly)
            {
                if (!IsDarkModeActive)
                {
                    lockMessage = "Umbra Mode Required to Interact ([F] Flip)";
                    return false;
                }
                return true;
            }
        }

        if (IsDarkModeActive)
        {
            lockMessage = "Light Mode Required to Interact ([F] Flip)";
            return false;
        }
        return true;
    }
}
