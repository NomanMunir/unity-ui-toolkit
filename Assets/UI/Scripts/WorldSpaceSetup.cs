/*
 * ============================================
 *  WorldSpaceSetup.cs — Scene Auto-Setup
 * ============================================
 *
 *  This is a HELPER script that creates the demo scene at runtime.
 *  It spawns NPC cubes, attaches UIDocuments, and configures
 *  PanelSettings for world-space rendering.
 *
 *  IN A REAL GAME:
 *  You would NOT auto-create NPCs like this. Instead, your NPC
 *  prefabs would already have UIDocument + WorldSpaceUIController
 *  components. This script exists purely for the learning demo.
 *
 *  THIS SCRIPT TEACHES:
 *  ─────────────────────────────────────────────
 *  1. CREATING PANEL SETTINGS FOR WORLD SPACE
 *     PanelSettings.renderMode must be set to WorldSpace.
 *     pixelsPerUnit controls how large the UI appears in 3D.
 *
 *  2. ATTACHING UIDocument TO GAMEOBJECTS
 *     Each NPC gets its own UIDocument + PanelSettings instance.
 *     In world space, each UIDocument IS a separate 3D panel.
 *
 *  3. SIMPLE FREE-CAMERA CONTROLLER
 *     WASD + mouse to fly around and inspect the world-space UI
 *     from different distances and angles.
 */

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Spawns demo NPCs with world-space UI and provides a free-fly camera.
/// Add this to an empty GameObject in a new scene.
/// </summary>
public class WorldSpaceSetup : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────

    [Header("World Space UI Assets")]
    [Tooltip("The UXML template for NPC health bars (WorldSpaceNPC.uxml)")]
    [SerializeField] private VisualTreeAsset npcUITemplate;

    [Tooltip("Panel Settings configured for World Space rendering")]
    [SerializeField] private PanelSettings worldSpacePanelSettings;

    [Header("Demo NPC Config")]
    [SerializeField] private float uiHeightOffset = 1.8f;

    [Header("Camera Control")]
    [SerializeField] private float moveSpeed   = 8f;
    [SerializeField] private float lookSpeed   = 3f;
    [SerializeField] private float sprintMultiplier = 2.5f;

    // ─────────────────────────────────────────
    //  PRIVATE STATE
    // ─────────────────────────────────────────
    private float _rotX;
    private float _rotY;


    // ═══════════════════════════════════════════
    //  SETUP
    // ═══════════════════════════════════════════

    private void Start()
    {
        /*
         * SCENE SETUP ORDER:
         * 1. Create the ground plane
         * 2. Spawn NPC cubes at predefined positions
         * 3. Attach world-space UI to each NPC
         * 4. Position the camera
         */

        SetupEnvironment();
        SpawnNPCs();
        SetupCamera();

        Debug.Log("═══════════════════════════════════════════");
        Debug.Log("  WORLD SPACE UI DEMO");
        Debug.Log("  WASD = Move  |  Mouse = Look  |  Shift = Sprint");
        Debug.Log("  Walk toward/away from NPCs to see distance fade");
        Debug.Log("═══════════════════════════════════════════");
    }


    // ─────────────────────────────────────────
    //  ENVIRONMENT
    // ─────────────────────────────────────────

    private void SetupEnvironment()
    {
        // Ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(5f, 1f, 5f);

        Renderer groundRenderer = ground.GetComponent<Renderer>();
        groundRenderer.material = new Material(Shader.Find("Standard"));
        groundRenderer.material.color = new Color(0.15f, 0.18f, 0.25f);

        // Directional light
        if (FindObjectOfType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.95f, 0.9f);
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // Skybox-ish background (set camera clear color)
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
    }


    // ─────────────────────────────────────────
    //  NPC SPAWNING
    // ─────────────────────────────────────────

    /// <summary>
    /// Spawns 3 NPC cubes and attaches world-space UI to each.
    ///
    /// ARCHITECTURE NOTE:
    /// Each NPC has TWO GameObjects:
    ///   1. The NPC body (cube) — at ground level
    ///   2. The UI anchor (child) — offset above the NPC's head
    ///
    /// The UIDocument is on the CHILD so the UI floats above the NPC.
    /// In a real game, this would be part of your NPC prefab.
    /// </summary>
    private void SpawnNPCs()
    {
        // NPC definitions: (name, position, color)
        var npcs = new (string name, Vector3 pos, Color color)[]
        {
            ("Knight",  new Vector3(-4f, 0.5f,  4f), new Color(0.3f, 0.5f, 0.9f)),
            ("Archer",  new Vector3( 0f, 0.5f,  7f), new Color(0.2f, 0.8f, 0.4f)),
            ("Mage",    new Vector3( 4f, 0.5f,  3f), new Color(0.7f, 0.3f, 0.9f)),
        };

        foreach (var (npcName, pos, color) in npcs)
        {
            SpawnSingleNPC(npcName, pos, color);
        }
    }

    /// <summary>
    /// Creates one NPC with a world-space UI panel above its head.
    ///
    /// WORLD-SPACE UI SETUP STEPS:
    /// 1. Create PanelSettings with renderMode = WorldSpace
    /// 2. Create a child GameObject above the NPC
    /// 3. Add UIDocument component to the child
    /// 4. Assign the PanelSettings and UXML template
    /// 5. Add WorldSpaceUIController for logic
    /// </summary>
    private void SpawnSingleNPC(string npcName, Vector3 position, Color color)
    {
        // --- 1. Create the NPC body ---
        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Cube);
        npc.name = $"NPC_{npcName}";
        npc.transform.position = position;

        Renderer renderer = npc.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = color;

        // --- 2. Create the UI anchor (child, offset above head) ---
        /*
         * WHY A CHILD GAMEOBJECT?
         *
         * The UIDocument renders at the GameObject's transform position.
         * We want the UI ABOVE the NPC's head, not at its center.
         * A child object with a Y offset achieves this cleanly.
         *
         * Alternative: You could offset in USS (margin-bottom: X),
         * but a transform offset is more intuitive in 3D.
         */
        GameObject uiAnchor = new GameObject($"{npcName}_UI");
        uiAnchor.transform.SetParent(npc.transform);
        uiAnchor.transform.localPosition = new Vector3(0f, uiHeightOffset, 0f);

        // --- 3. Add UIDocument ---
        UIDocument uiDoc = uiAnchor.AddComponent<UIDocument>();

        /*
         * PANEL SETTINGS — THE KEY TO WORLD SPACE:
         *
         * If worldSpacePanelSettings is assigned in the Inspector,
         * use it. Otherwise, the UIDocument will use its default
         * PanelSettings (which you should configure as World Space
         * in the Unity Inspector).
         *
         * IMPORTANT: In Unity 6+, set these on the PanelSettings asset:
         *   - Render Mode: World Space
         *   - Pixels Per Unit: 100 (default, adjust to taste)
         *
         * LEGACY (Unity 2022 LTS):
         *   World Space render mode isn't available. Instead:
         *   1. Create a RenderTexture (e.g., 256×128)
         *   2. Set PanelSettings.targetTexture = that RenderTexture
         *   3. Create a Quad in the scene
         *   4. Assign the RenderTexture to the Quad's material
         *   5. Position the Quad above the NPC
         *   This is more manual but achieves the same visual result.
         */
        if (worldSpacePanelSettings != null)
        {
            uiDoc.panelSettings = worldSpacePanelSettings;
        }

        // Assign the UXML template
        if (npcUITemplate != null)
        {
            uiDoc.visualTreeAsset = npcUITemplate;
        }

        // --- 4. Add the controller script ---
        WorldSpaceUIController controller = uiAnchor.AddComponent<WorldSpaceUIController>();

        // Configure via serialized fields (uses reflection for demo)
        SetPrivateField(controller, "npcName", npcName);
        SetPrivateField(controller, "maxHealth", 100f);
        SetPrivateField(controller, "currentHealth", 100f);
        SetPrivateField(controller, "runDemo", true);
    }

    /// <summary>
    /// Helper to set serialized private fields at runtime.
    /// In a real game, use prefabs instead of this reflection hack!
    /// </summary>
    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );
        field?.SetValue(target, value);
    }


    // ═══════════════════════════════════════════
    //  FREE-FLY CAMERA
    //
    //  Uses the NEW Input System (UnityEngine.InputSystem)
    //  Unity 6+ defaults to this — the legacy Input class
    //  throws an error if the project uses Input System Package.
    // ═══════════════════════════════════════════

    /// <summary>
    /// Positions the camera to look at the NPC group.
    /// </summary>
    private void SetupCamera()
    {
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(0f, 3f, -5f);
        cam.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

        _rotX = 15f;
        _rotY = 0f;

        // Lock cursor for mouse-look
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    /// <summary>
    /// WASD + mouse free-fly camera controller.
    /// Walk toward NPCs to see the distance fade in action!
    ///
    /// Press Escape to unlock the cursor.
    ///
    /// NEW INPUT SYSTEM NOTE:
    /// Instead of Input.GetKey(KeyCode.W), we use:
    ///   Keyboard.current.wKey.isPressed
    /// Instead of Input.GetAxis("Mouse X"), we use:
    ///   Mouse.current.delta.ReadValue()
    /// </summary>
    private void Update()
    {
        HandleCameraInput();
    }

    private void HandleCameraInput()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (keyboard == null || mouse == null) return;

        Transform camTransform = cam.transform;

        // --- Escape to toggle cursor lock ---
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            bool locked = UnityEngine.Cursor.lockState == CursorLockMode.Locked;
            UnityEngine.Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            UnityEngine.Cursor.visible = locked;
        }

        // --- Mouse Look ---
        if (UnityEngine.Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            _rotY += mouseDelta.x * lookSpeed * 0.1f;
            _rotX -= mouseDelta.y * lookSpeed * 0.1f;
            _rotX = Mathf.Clamp(_rotX, -89f, 89f);

            camTransform.rotation = Quaternion.Euler(_rotX, _rotY, 0f);
        }

        // --- Movement (WASD) ---
        float speed = moveSpeed * (keyboard.leftShiftKey.isPressed ? sprintMultiplier : 1f);
        Vector3 move = Vector3.zero;

        if (keyboard.wKey.isPressed) move += camTransform.forward;
        if (keyboard.sKey.isPressed) move -= camTransform.forward;
        if (keyboard.aKey.isPressed) move -= camTransform.right;
        if (keyboard.dKey.isPressed) move += camTransform.right;
        if (keyboard.eKey.isPressed) move += Vector3.up;
        if (keyboard.qKey.isPressed) move -= Vector3.up;

        camTransform.position += move.normalized * speed * Time.deltaTime;
    }
}

