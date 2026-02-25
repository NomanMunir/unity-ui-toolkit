/*
 * ============================================
 *  WorldSpaceUIController.cs — Per-NPC World-Space UI
 * ============================================
 *
 *  Attach this to any GameObject that has a UIDocument component.
 *  It controls the floating NPC health bar / status panel.
 *
 *  THIS SCRIPT TEACHES:
 *  ─────────────────────────────────────────────
 *  1. BILLBOARD ROTATION
 *     World-space UI panels are 3D objects. By default they face
 *     one direction. To make them always face the camera (like a
 *     classic MMO health bar), we rotate the transform every frame.
 *
 *  2. DISTANCE-BASED FADING
 *     World-space UI can clutter the screen if every NPC's label
 *     is always visible. We fade the panel's opacity based on
 *     distance from the camera — invisible when far, fully visible
 *     when close.
 *
 *  3. INLINE STYLES FOR CONTINUOUS VALUES
 *     Health percentage changes smoothly over time. Use inline
 *     styles (element.style.width) for continuous/animated values.
 *
 *  4. USS CLASSES FOR DISCRETE STATES
 *     NPC status (Patrol/Alert/Combat) is a discrete state.
 *     Use AddToClassList/RemoveFromClassList to switch styles.
 *     This keeps your C# clean and your styles in USS.
 *
 *  5. WORLD-SPACE UI POSITIONING
 *     The UIDocument renders at the GameObject's transform position.
 *     We offset it above the NPC using a local Y offset.
 */

using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

/// <summary>
/// NPC status states — each maps to a USS class.
/// </summary>
public enum NPCStatus
{
    Patrol,
    Alert,
    Combat
}

/// <summary>
/// Controls a single NPC's world-space UI panel.
/// Handles billboard rotation, distance fading, health updates,
/// and status state management.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class WorldSpaceUIController : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────

    [Header("NPC Identity")]
    [SerializeField] private string npcName = "Guard";

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Billboard Settings")]
    [Tooltip("If true, the UI always faces the camera (billboard effect)")]
    [SerializeField] private bool billboard = true;

    [Header("Distance Fade")]
    [Tooltip("Distance at which the UI starts fading out")]
    [SerializeField] private float fadeStartDistance = 15f;
    [Tooltip("Distance at which the UI is fully invisible")]
    [SerializeField] private float fadeEndDistance = 25f;

    [Header("Demo Mode")]
    [Tooltip("Auto-animate health and status changes for testing")]
    [SerializeField] private bool runDemo = true;

    // ─────────────────────────────────────────
    //  CACHED UI REFERENCES
    // ─────────────────────────────────────────
    //
    //  PATTERN: Cache Q() results in OnEnable, never in Update.
    //  Q() searches the visual tree each call — expensive if repeated.
    //
    private VisualElement _root;
    private Label         _nameLabel;
    private VisualElement _healthFill;
    private Label         _healthText;
    private VisualElement _statusDot;
    private Label         _statusLabel;

    // ─────────────────────────────────────────
    //  STATE
    // ─────────────────────────────────────────
    private NPCStatus _currentStatus = NPCStatus.Patrol;
    private Camera    _mainCamera;

    // Status class names — must match the USS class names exactly
    private static readonly string[] StatusClasses = {
        "status-patrol",
        "status-alert",
        "status-combat"
    };

    // Human-readable status labels
    private static readonly string[] StatusLabels = {
        "Patrolling",
        "Alert",
        "In Combat"
    };


    // ═══════════════════════════════════════════
    //  LIFECYCLE
    // ═══════════════════════════════════════════

    private void OnEnable()
    {
        /*
         * WORLD-SPACE UI INITIALIZATION:
         *
         * Unlike screen-space UI (where the UIDocument renders to the
         * full screen), world-space UIDocuments render at the
         * GameObject's transform position in 3D.
         *
         * The PanelSettings asset controls HOW it renders:
         *   - renderMode = WorldSpace  (set on the PanelSettings asset)
         *   - pixelsPerUnit = 100      (how many USS pixels = 1 Unity unit)
         *
         * We still query elements the exact same way as screen-space!
         */

        _mainCamera = Camera.main;

        UIDocument uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        CacheUIReferences();
        InitializeUI();

        if (runDemo)
        {
            StartCoroutine(DemoRoutine());
        }
    }

    /// <summary>
    /// Update runs every frame for billboard rotation and distance fading.
    ///
    /// NOTE ON PERFORMANCE:
    /// For a few NPCs, Update() is fine. For hundreds, consider using
    /// a manager pattern that iterates over all NPCs in a single Update().
    /// </summary>
    private void Update()
    {
        if (_mainCamera == null) return;

        if (billboard)
        {
            ApplyBillboard();
        }

        ApplyDistanceFade();
    }


    // ═══════════════════════════════════════════
    //  UI QUERY & INITIALIZE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Cache all UI element references.
    /// Q() is the UI Toolkit equivalent of transform.Find() —
    /// it searches by name attribute in the UXML.
    /// </summary>
    private void CacheUIReferences()
    {
        _nameLabel  = _root.Q<Label>("npc-name");
        _healthFill = _root.Q<VisualElement>("health-bar-fill");
        _healthText = _root.Q<Label>("health-text");
        _statusDot  = _root.Q<VisualElement>("status-dot");
        _statusLabel = _root.Q<Label>("status-label");
    }

    private void InitializeUI()
    {
        _nameLabel.text = npcName;
        UpdateHealthBar();
        SetStatus(NPCStatus.Patrol);
    }


    // ═══════════════════════════════════════════
    //  BILLBOARD
    // ═══════════════════════════════════════════

    /// <summary>
    /// Makes the UI panel always face the camera.
    ///
    /// HOW IT WORKS:
    /// World-space UIDocuments render on a quad that faces the
    /// transform's forward direction. By rotating the transform
    /// to look at the camera each frame, the UI always faces you.
    ///
    /// We use the camera's rotation directly (not LookAt) to avoid
    /// perspective distortion at steep angles.
    /// </summary>
    private void ApplyBillboard()
    {
        /*
         * WHY camera.rotation INSTEAD OF LookAt()?
         *
         * LookAt() points the Z-axis toward a position, which causes
         * slight perspective skewing when viewing from angles.
         *
         * Copying the camera's rotation ensures ALL billboards face
         * the exact same direction — no per-panel distortion.
         * This is how most games do it (e.g., particle billboards).
         */
        transform.rotation = _mainCamera.transform.rotation;
    }


    // ═══════════════════════════════════════════
    //  DISTANCE FADE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Fades the UI panel based on distance from camera.
    ///
    /// WHY? In a game with many NPCs, you don't want 50 health bars
    /// cluttering the screen. Only show nearby NPC labels.
    ///
    /// HOW:
    /// 1. Calculate distance from camera to this NPC
    /// 2. If within fadeStartDistance → fully visible (opacity 1)
    /// 3. If beyond fadeEndDistance → invisible (opacity 0)
    /// 4. In between → lerp the opacity smoothly
    ///
    /// We use Mathf.InverseLerp for clean 0→1 mapping.
    /// </summary>
    private void ApplyDistanceFade()
    {
        float distance = Vector3.Distance(
            _mainCamera.transform.position,
            transform.position
        );

        /*
         * InverseLerp(a, b, value):
         * Returns 0 when value == a, 1 when value == b.
         *
         * We REVERSE it (1 - result) because:
         *   - Close = 1.0 (fully visible)
         *   - Far   = 0.0 (invisible)
         */
        float alpha = 1f - Mathf.InverseLerp(fadeStartDistance, fadeEndDistance, distance);

        // Apply opacity via inline style
        _root.style.opacity = alpha;
    }


    // ═══════════════════════════════════════════
    //  HEALTH BAR
    // ═══════════════════════════════════════════

    /// <summary>
    /// Updates the health bar fill width and text.
    ///
    /// INLINE STYLES vs USS:
    /// The health PERCENTAGE changes continuously, so we use an
    /// inline style. USS transitions (defined in WorldSpace.uss)
    /// handle the smooth animation automatically.
    /// </summary>
    public void UpdateHealthBar()
    {
        float percent = Mathf.Clamp01(currentHealth / maxHealth) * 100f;

        // Set fill width — USS transition handles smooth animation
        _healthFill.style.width = new Length(percent, LengthUnit.Percent);

        // Update text
        _healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";

        // Color based on health level
        // LOW health = red, MEDIUM = orange, HIGH = green
        Color barColor;
        if (percent <= 25f)
            barColor = new Color(1f, 0.32f, 0.32f);      // Red — critical
        else if (percent <= 50f)
            barColor = new Color(1f, 0.84f, 0.25f);       // Orange — warning
        else
            barColor = new Color(0.41f, 0.94f, 0.68f);    // Green — healthy

        _healthFill.style.backgroundColor = barColor;
    }

    /// <summary>
    /// Public API — call this from your game logic.
    /// </summary>
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        UpdateHealthBar();
    }

    /// <summary>
    /// Applies damage and updates the UI.
    /// </summary>
    public void TakeDamage(float damage)
    {
        SetHealth(currentHealth - damage);
    }

    /// <summary>
    /// Heals the NPC and updates the UI.
    /// </summary>
    public void Heal(float amount)
    {
        SetHealth(currentHealth + amount);
    }


    // ═══════════════════════════════════════════
    //  STATUS STATE MACHINE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Changes the NPC status and updates the visual indicator.
    ///
    /// USS CLASS TOGGLING PATTERN:
    /// 1. Remove the OLD state class
    /// 2. Add the NEW state class
    ///
    /// This keeps your C# code clean — all the visual styling
    /// (colors, sizes, etc.) lives in the USS file.
    ///
    /// Compare this to the health bar, where we set colors inline
    /// because it's a continuous gradient, not a discrete state.
    /// </summary>
    public void SetStatus(NPCStatus newStatus)
    {
        // Remove old status class
        _statusDot.RemoveFromClassList(StatusClasses[(int)_currentStatus]);

        // Set new status
        _currentStatus = newStatus;

        // Add new status class — USS defines the color
        _statusDot.AddToClassList(StatusClasses[(int)_currentStatus]);

        // Update label text
        _statusLabel.text = StatusLabels[(int)_currentStatus];
    }


    // ═══════════════════════════════════════════
    //  DEMO ROUTINE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Auto-plays a demo that cycles health and status.
    /// Remove this in a real game — it's just for testing!
    /// </summary>
    private IEnumerator DemoRoutine()
    {
        // Stagger start so NPCs don't animate in sync
        yield return new WaitForSeconds(Random.Range(0f, 2f));

        while (true)
        {
            // Phase 1: Patrol — slowly lose health
            SetStatus(NPCStatus.Patrol);
            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.8f, 1.5f));
                TakeDamage(Random.Range(3f, 8f));
            }

            // Phase 2: Alert — take moderate damage
            SetStatus(NPCStatus.Alert);
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));
                TakeDamage(Random.Range(8f, 15f));
            }

            // Phase 3: Combat — heavy damage
            SetStatus(NPCStatus.Combat);
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
                TakeDamage(Random.Range(10f, 20f));
            }

            // Phase 4: Recovery — heal back up
            SetStatus(NPCStatus.Patrol);
            while (currentHealth < maxHealth)
            {
                yield return new WaitForSeconds(0.3f);
                Heal(Random.Range(5f, 10f));
            }

            // Brief pause before restarting
            yield return new WaitForSeconds(2f);
        }
    }
}
