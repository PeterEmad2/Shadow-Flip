using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class logic : MonoBehaviour
{
    public static logic Instance { get; private set; }

    public GameObject main_menu;
    public bool isDarkActive = false;

    [Header("World Lighting (Optional)")]
    public Light2D globalLight;
    public Color lightWorldColor = new Color(1f, 0.98f, 0.92f, 1f);
    public float lightWorldIntensity = 1f;
    public Color shadowWorldColor = new Color(0.45f, 0.52f, 0.85f, 1f);
    public float shadowWorldIntensity = 0.55f;
    public float lightTransitionSpeed = 8f;

    [Header("Controls")]
    public KeyCode restartKey = KeyCode.R;

    public event Action<bool> OnWorldFlipped;

    private bool isPaused = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (main_menu != null && main_menu.activeSelf)
        {
            pause();
        }
        else
        {
            Continue();
        }

        ApplyWorldLightingImmediate();
        OnWorldFlipped?.Invoke(isDarkActive);
    }

    void Update()
    {
        if (!isPaused && Input.GetKeyDown(restartKey))
        {
            RestartLevel();
        }

        UpdateWorldLighting();
    }

    public void SetWorld(bool darkActive)
    {
        if (isDarkActive == darkActive) return;
        isDarkActive = darkActive;
        OnWorldFlipped?.Invoke(isDarkActive);
    }

    public void FlipWorld()
    {
        SetWorld(!isDarkActive);
    }

    public void Play()
    {
        if (main_menu != null)
        {
            main_menu.SetActive(false);
        }
        Continue();
    }

    public void pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void Continue()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ApplyWorldLightingImmediate()
    {
        if (globalLight == null) return;
        globalLight.color = isDarkActive ? shadowWorldColor : lightWorldColor;
        globalLight.intensity = isDarkActive ? shadowWorldIntensity : lightWorldIntensity;
    }

    private void UpdateWorldLighting()
    {
        if (globalLight == null) return;
        Color targetColor = isDarkActive ? shadowWorldColor : lightWorldColor;
        float targetIntensity = isDarkActive ? shadowWorldIntensity : lightWorldIntensity;

        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.unscaledDeltaTime * lightTransitionSpeed);
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.unscaledDeltaTime * lightTransitionSpeed);
    }
}
