/*
 * ============================================
 *  HUDController.cs — In-Game HUD Logic
 * ============================================
 *
 *  This script demonstrates:
 *  1. Updating UI elements in real-time (Update loop)
 *  2. Animating bar widths via inline styles
 *  3. Showing/hiding notifications
 *  4. Managing pause state
 *  5. Using coroutines with UI Toolkit
 *
 *  INLINE STYLES vs USS CLASSES:
 *  ─────────────────────────────────────────
 *  USS (classes): For static/design styles → element.AddToClassList("class")
 *  Inline (C#):   For dynamic runtime values → element.style.width = new Length(50, LengthUnit.Percent)
 *
 *  Inline styles OVERRIDE USS styles (like inline style="" in HTML)
 */

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controls the in-game HUD — health bars, score,
/// ability cooldowns, notifications, and pause.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class HUDController : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [Header("Demo Settings")]
    [Tooltip("Enable to auto-run a demo that changes health/score over time")]
    [SerializeField] private bool runDemo = true;

    // ─────────────────────────────────────────
    //  CACHED UI REFERENCES
    // ─────────────────────────────────────────
    private VisualElement _root;

    // Health & Mana bars
    private VisualElement _healthBarFill;
    private VisualElement _manaBarFill;
    private Label _healthText;
    private Label _manaText;

    // Score & Timer
    private Label _scoreValue;
    private Label _timerValue;

    // Notifications
    private Label _notificationText;

    // Pause
    private VisualElement _pauseOverlay;

    // Ability cooldowns
    private VisualElement[] _cooldownOverlays = new VisualElement[4];

    // ─────────────────────────────────────────
    //  GAME STATE (simulated for demo)
    // ─────────────────────────────────────────
    private float _currentHealth = 85f;
    private float _maxHealth = 100f;
    private float _currentMana = 60f;
    private float _maxMana = 100f;
    private int   _score = 4500;
    private float _gameTime = 754f; // seconds (12:34)
    private bool  _isPaused = false;


    // ─────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────

    private void OnEnable()
    {
        UIDocument uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        CacheUIReferences();
        RegisterEvents();
        UpdateAllUI();

        if (runDemo)
        {
            StartCoroutine(DemoRoutine());
        }
    }

    /// <summary>
    /// Update is called every frame.
    /// We use it to update the timer display.
    /// NOTE: Only update UI in Update when necessary!
    /// For most things, use events/callbacks instead.
    /// </summary>
    private void Update()
    {
        if (_isPaused) return;

        // Update game timer
        _gameTime += Time.deltaTime;
        UpdateTimerDisplay();
    }


    // ─────────────────────────────────────────
    //  QUERY & CACHE
    // ─────────────────────────────────────────

    private void CacheUIReferences()
    {
        // Health & Mana
        _healthBarFill = _root.Q<VisualElement>("health-bar-fill");
        _manaBarFill   = _root.Q<VisualElement>("mana-bar-fill");
        _healthText    = _root.Q<Label>("health-text");
        _manaText      = _root.Q<Label>("mana-text");

        // Score & Timer
        _scoreValue = _root.Q<Label>("score-value");
        _timerValue = _root.Q<Label>("timer-value");

        // Notification
        _notificationText = _root.Q<Label>("notification-text");

        // Pause
        _pauseOverlay = _root.Q<VisualElement>("pause-overlay");

        // Cooldown overlays
        for (int i = 0; i < 4; i++)
        {
            _cooldownOverlays[i] = _root.Q<VisualElement>($"cooldown-{i + 1}");
        }
    }


    // ─────────────────────────────────────────
    //  EVENT REGISTRATION
    // ─────────────────────────────────────────

    private void RegisterEvents()
    {
        // Pause button
        _root.Q<Button>("btn-pause").clicked += TogglePause;

        // Pause menu buttons
        _root.Q<Button>("btn-resume").clicked += TogglePause;
        _root.Q<Button>("btn-main-menu").clicked += GoToMainMenu;

        // Ability slots — register click handlers
        for (int i = 1; i <= 4; i++)
        {
            int abilityIndex = i; // Capture for closure
            VisualElement slot = _root.Q<VisualElement>($"ability-{i}");
            slot.RegisterCallback<ClickEvent>(evt => UseAbility(abilityIndex));
        }
    }


    // ─────────────────────────────────────────
    //  UI UPDATE METHODS
    //  These show how to set INLINE styles via C#
    // ─────────────────────────────────────────

    /// <summary>
    /// Updates all UI elements to match current game state.
    /// </summary>
    private void UpdateAllUI()
    {
        UpdateHealthBar();
        UpdateManaBar();
        UpdateScoreDisplay();
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Updates the health bar width and text.
    ///
    /// INLINE STYLE EXAMPLE:
    /// element.style.width = new Length(percentage, LengthUnit.Percent);
    ///
    /// This sets an inline style that overrides USS, just like
    /// element.style.width = "85%" in JavaScript.
    /// </summary>
    public void UpdateHealthBar()
    {
        float percentage = (_currentHealth / _maxHealth) * 100f;

        // Set bar width using inline style (overrides USS)
        _healthBarFill.style.width = new Length(percentage, LengthUnit.Percent);

        // Update text
        _healthText.text = $"{_currentHealth:F0}/{_maxHealth:F0}";

        // Change color based on health level
        if (percentage <= 25f)
            _healthBarFill.style.backgroundColor = new Color(1f, 0.2f, 0.2f); // Critical red
        else if (percentage <= 50f)
            _healthBarFill.style.backgroundColor = new Color(1f, 0.6f, 0.2f); // Warning orange
        else
            _healthBarFill.style.backgroundColor = new Color(1f, 0.32f, 0.32f); // Normal red
    }

    /// <summary>
    /// Updates the mana bar width and text.
    /// </summary>
    public void UpdateManaBar()
    {
        float percentage = (_currentMana / _maxMana) * 100f;
        _manaBarFill.style.width = new Length(percentage, LengthUnit.Percent);
        _manaText.text = $"{_currentMana:F0}/{_maxMana:F0}";
    }

    /// <summary>
    /// Updates the score display with formatting.
    /// </summary>
    public void UpdateScoreDisplay()
    {
        // Format with comma separator: 4500 → "04,500"
        _scoreValue.text = _score.ToString("N0").PadLeft(6, '0');
    }

    /// <summary>
    /// Updates the timer display.
    /// </summary>
    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(_gameTime / 60f);
        int seconds = Mathf.FloorToInt(_gameTime % 60f);
        _timerValue.text = $"{minutes:D2}:{seconds:D2}";
    }


    // ─────────────────────────────────────────
    //  GAME ACTIONS
    // ─────────────────────────────────────────

    /// <summary>
    /// Sets health and updates the UI.
    /// Call this from your game logic.
    /// </summary>
    public void SetHealth(float health)
    {
        _currentHealth = Mathf.Clamp(health, 0, _maxHealth);
        UpdateHealthBar();
    }

    /// <summary>
    /// Sets mana and updates the UI.
    /// </summary>
    public void SetMana(float mana)
    {
        _currentMana = Mathf.Clamp(mana, 0, _maxMana);
        UpdateManaBar();
    }

    /// <summary>
    /// Adds to the score and updates the UI.
    /// </summary>
    public void AddScore(int points)
    {
        _score += points;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Simulates using an ability with a cooldown visual.
    /// </summary>
    private void UseAbility(int abilityIndex)
    {
        Debug.Log($"Ability {abilityIndex} used!");
        ShowNotification($"Ability {abilityIndex} activated!");

        // Start cooldown animation
        StartCoroutine(CooldownRoutine(abilityIndex - 1, 3f));
    }

    /// <summary>
    /// Animates a cooldown overlay on an ability slot.
    ///
    /// This demonstrates animating UI with coroutines:
    /// - Set height from 100% to 0% over time
    /// - Uses inline styles for dynamic values
    /// </summary>
    private IEnumerator CooldownRoutine(int slotIndex, float duration)
    {
        VisualElement overlay = _cooldownOverlays[slotIndex];
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!_isPaused)
            {
                elapsed += Time.deltaTime;
                float remaining = 1f - (elapsed / duration);

                // Animate height from 100% down to 0%
                overlay.style.height = new Length(remaining * 100f, LengthUnit.Percent);
            }
            yield return null;
        }

        // Cooldown complete — fully hide overlay
        overlay.style.height = new Length(0, LengthUnit.Percent);
    }


    // ─────────────────────────────────────────
    //  NOTIFICATIONS
    // ─────────────────────────────────────────

    /// <summary>
    /// Shows a notification message that fades out.
    ///
    /// This demonstrates manipulating opacity via inline styles
    /// combined with USS transitions for smooth animation.
    /// </summary>
    public void ShowNotification(string message)
    {
        StopCoroutine(nameof(NotificationRoutine));
        StartCoroutine(NotificationRoutine(message));
    }

    private IEnumerator NotificationRoutine(string message)
    {
        _notificationText.text = message;
        _notificationText.style.opacity = 1f;  // Show (USS transition handles fade)

        yield return new WaitForSecondsRealtime(2f);

        _notificationText.style.opacity = 0f;  // Fade out (USS transition handles animation)
    }


    // ─────────────────────────────────────────
    //  PAUSE SYSTEM
    // ─────────────────────────────────────────

    /// <summary>
    /// Toggles the pause state and overlay visibility.
    ///
    /// Time.timeScale = 0 pauses the game.
    /// We use WaitForSecondsRealtime in coroutines
    /// because regular WaitForSeconds respects timeScale.
    /// </summary>
    private void TogglePause()
    {
        _isPaused = !_isPaused;

        if (_isPaused)
        {
            _pauseOverlay.RemoveFromClassList("hidden");
            Time.timeScale = 0f;
        }
        else
        {
            _pauseOverlay.AddToClassList("hidden");
            Time.timeScale = 1f;
        }
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reset before scene change!
        SceneManager.LoadScene(mainMenuSceneName);
    }


    // ─────────────────────────────────────────
    //  DEMO MODE
    //  Auto-simulates gameplay to preview the HUD
    // ─────────────────────────────────────────

    /// <summary>
    /// Runs a demo that simulates gameplay events.
    /// This is purely for testing — remove in real game!
    /// </summary>
    private IEnumerator DemoRoutine()
    {
        yield return new WaitForSeconds(1f);
        ShowNotification("⚔ Game Started!");

        while (true)
        {
            if (!_isPaused)
            {
                // Simulate taking damage
                yield return new WaitForSeconds(2f);
                SetHealth(_currentHealth - Random.Range(5f, 15f));
                AddScore(Random.Range(100, 500));

                // Simulate using mana
                yield return new WaitForSeconds(1.5f);
                SetMana(_currentMana - Random.Range(5f, 10f));

                // Simulate healing
                yield return new WaitForSeconds(3f);
                SetHealth(_currentHealth + Random.Range(10f, 20f));
                SetMana(_currentMana + Random.Range(5f, 15f));

                // Show random notification
                if (Random.value > 0.7f)
                {
                    string[] messages = {
                        "⚔ Enemy Defeated! +200 XP",
                        "🛡 Shield Activated!",
                        "🧪 Health Potion Used",
                        "⭐ Level Up!",
                        "💎 Rare Item Found!"
                    };
                    ShowNotification(messages[Random.Range(0, messages.Length)]);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// Clean up when destroyed — reset time scale.
    /// </summary>
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
