/*
 * ============================================
 *  MainMenuController.cs — Main Menu Logic
 * ============================================
 *
 *  HOW UI TOOLKIT C# WORKS:
 *  ─────────────────────────────────────────
 *  1. Get the UIDocument component (holds the UXML)
 *  2. Access the rootVisualElement (top of the visual tree)
 *  3. Query elements by name using Q<Type>("name") or Q("name")
 *  4. Register event callbacks (like addEventListener in JS)
 *
 *  QUERY METHODS:
 *  ─────────────────────────────────────────
 *  root.Q<Button>("btn-play")        → Find single element by name & type
 *  root.Q("btn-play")                → Find single element by name (any type)
 *  root.Q<Button>(className: "cls")  → Find single element by class
 *  root.Query<Button>().ToList()     → Find ALL matching elements
 *
 *  EVENT SYSTEM:
 *  ─────────────────────────────────────────
 *  button.clicked += () => { ... }           → Simple click handler
 *  button.RegisterCallback<ClickEvent>(e => { ... })  → Detailed click event
 *  slider.RegisterValueChangedCallback(e => { ... })  → Value change event
 *
 *  SETUP IN UNITY:
 *  ─────────────────────────────────────────
 *  1. Create an empty GameObject in your scene
 *  2. Add a UIDocument component to it
 *  3. Assign the UXML file to the UIDocument's "Source Asset" field
 *  4. Add this script to the SAME GameObject
 *  5. The [RequireComponent] attribute ensures UIDocument exists
 */

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the Main Menu UI — handles navigation between
/// menu panels and button interactions.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  INSPECTOR FIELDS
    //  Assign these in the Unity Inspector
    // ─────────────────────────────────────────
    [Header("UI Documents")]
    [Tooltip("The UXML document for the main menu")]
    [SerializeField] private VisualTreeAsset menuDocument;

    [Header("Scene Names")]
    [Tooltip("Name of the game scene to load when Play is clicked")]
    [SerializeField] private string gameSceneName = "GameScene";

    // ─────────────────────────────────────────
    //  CACHED UI REFERENCES
    //  We query these once and reuse them
    // ─────────────────────────────────────────
    private VisualElement _root;
    private VisualElement _mainMenu;
    private VisualElement _settingsPanel;
    private VisualElement _aboutPanel;

    // Settings controls
    private Slider _masterVolumeSlider;
    private Slider _musicVolumeSlider;
    private Slider _sfxVolumeSlider;
    private Label  _masterVolumeLabel;
    private Label  _musicVolumeLabel;
    private Label  _sfxVolumeLabel;
    private Toggle _fullscreenToggle;
    private Toggle _vsyncToggle;
    private DropdownField _qualityDropdown;


    // ─────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────

    /// <summary>
    /// Called when the script starts.
    /// This is where we query the UI and register all events.
    /// Think of this as document.addEventListener('DOMContentLoaded', ...) in JS
    /// </summary>
    private void OnEnable()
    {
        // Step 1: Get the root of the visual tree
        // The UIDocument component holds our UXML, and its rootVisualElement
        // is the top-level container (like document.body in HTML)
        UIDocument uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        // Step 2: Query and cache UI elements
        CacheUIReferences();

        // Step 3: Register button click handlers
        RegisterButtonEvents();

        // Step 4: Register settings control events
        RegisterSettingsEvents();

        // Step 5: Show main menu by default
        ShowPanel(PanelType.MainMenu);
    }


    // ─────────────────────────────────────────
    //  QUERY & CACHE — Finding UI Elements
    // ─────────────────────────────────────────

    /// <summary>
    /// Queries all UI elements we need and stores references.
    /// Always do this in OnEnable/Start — never query in Update!
    ///
    /// Q<T>("name") is like document.querySelector('#name') in JS
    /// </summary>
    private void CacheUIReferences()
    {
        // Panels
        _mainMenu       = _root.Q<VisualElement>("main-menu");
        _settingsPanel  = _root.Q<VisualElement>("settings-panel");
        _aboutPanel     = _root.Q<VisualElement>("about-panel");

        // Settings — Sliders
        _masterVolumeSlider = _root.Q<Slider>("slider-master-volume");
        _musicVolumeSlider  = _root.Q<Slider>("slider-music-volume");
        _sfxVolumeSlider    = _root.Q<Slider>("slider-sfx-volume");

        // Settings — Labels (showing current value)
        _masterVolumeLabel = _root.Q<Label>("label-master-volume");
        _musicVolumeLabel  = _root.Q<Label>("label-music-volume");
        _sfxVolumeLabel    = _root.Q<Label>("label-sfx-volume");

        // Settings — Toggles
        _fullscreenToggle = _root.Q<Toggle>("toggle-fullscreen");
        _vsyncToggle      = _root.Q<Toggle>("toggle-vsync");

        // Settings — Dropdown
        _qualityDropdown = _root.Q<DropdownField>("dropdown-quality");
    }


    // ─────────────────────────────────────────
    //  EVENT REGISTRATION — Button Clicks
    // ─────────────────────────────────────────

    /// <summary>
    /// Registers click handlers for all buttons.
    ///
    /// In UI Toolkit, there are two ways to handle clicks:
    /// 1. button.clicked += () => { }        — Simple, no event data
    /// 2. button.RegisterCallback<ClickEvent>(e => { })  — Full event data
    ///
    /// We use the simple approach here since we don't need event data.
    /// </summary>
    private void RegisterButtonEvents()
    {
        // Main Menu buttons
        _root.Q<Button>("btn-play").clicked     += OnPlayClicked;
        _root.Q<Button>("btn-settings").clicked += OnSettingsClicked;
        _root.Q<Button>("btn-about").clicked    += OnAboutClicked;
        _root.Q<Button>("btn-quit").clicked     += OnQuitClicked;

        // Settings panel buttons
        _root.Q<Button>("btn-apply-settings").clicked += OnApplySettings;
        _root.Q<Button>("btn-back-settings").clicked  += OnBackToMenu;

        // About panel button
        _root.Q<Button>("btn-back-about").clicked += OnBackToMenu;
    }


    // ─────────────────────────────────────────
    //  EVENT REGISTRATION — Settings Controls
    // ─────────────────────────────────────────

    /// <summary>
    /// Registers value-changed callbacks for sliders and toggles.
    ///
    /// RegisterValueChangedCallback gives you:
    /// - evt.newValue — the new value
    /// - evt.previousValue — the old value
    ///
    /// This is like the 'input' event on HTML range/checkbox elements.
    /// </summary>
    private void RegisterSettingsEvents()
    {
        // Slider callbacks — update the label text when slider moves
        _masterVolumeSlider.RegisterValueChangedCallback(evt =>
        {
            _masterVolumeLabel.text = $"{evt.newValue:F0}%";
        });

        _musicVolumeSlider.RegisterValueChangedCallback(evt =>
        {
            _musicVolumeLabel.text = $"{evt.newValue:F0}%";
        });

        _sfxVolumeSlider.RegisterValueChangedCallback(evt =>
        {
            _sfxVolumeLabel.text = $"{evt.newValue:F0}%";
        });

        // Toggle callbacks
        _fullscreenToggle.RegisterValueChangedCallback(evt =>
        {
            Debug.Log($"Fullscreen: {evt.newValue}");
        });

        _vsyncToggle.RegisterValueChangedCallback(evt =>
        {
            Debug.Log($"VSync: {evt.newValue}");
        });

        // Dropdown callback
        _qualityDropdown.RegisterValueChangedCallback(evt =>
        {
            Debug.Log($"Quality changed to: {evt.newValue}");
        });
    }


    // ─────────────────────────────────────────
    //  BUTTON HANDLERS
    // ─────────────────────────────────────────

    private void OnPlayClicked()
    {
        Debug.Log("Play button clicked — loading game scene...");

        // Load the game scene
        // Make sure 'GameScene' is added to Build Settings!
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnSettingsClicked()
    {
        ShowPanel(PanelType.Settings);
    }

    private void OnAboutClicked()
    {
        ShowPanel(PanelType.About);
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit button clicked — exiting application...");

        // Application.Quit() only works in a built game, not in the editor
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnApplySettings()
    {
        // Read current values from the UI controls
        float masterVolume = _masterVolumeSlider.value;
        float musicVolume  = _musicVolumeSlider.value;
        float sfxVolume    = _sfxVolumeSlider.value;
        bool  fullscreen   = _fullscreenToggle.value;
        bool  vsync        = _vsyncToggle.value;
        string quality     = _qualityDropdown.value;

        Debug.Log($"Settings Applied:");
        Debug.Log($"  Master: {masterVolume}%, Music: {musicVolume}%, SFX: {sfxVolume}%");
        Debug.Log($"  Fullscreen: {fullscreen}, VSync: {vsync}, Quality: {quality}");

        // Apply actual settings
        Screen.fullScreen = fullscreen;
        QualitySettings.vSyncCount = vsync ? 1 : 0;

        // Apply quality level
        switch (quality)
        {
            case "Low":    QualitySettings.SetQualityLevel(0); break;
            case "Medium": QualitySettings.SetQualityLevel(2); break;
            case "High":   QualitySettings.SetQualityLevel(4); break;
            case "Ultra":  QualitySettings.SetQualityLevel(5); break;
        }

        // Go back to main menu after applying
        ShowPanel(PanelType.MainMenu);
    }

    private void OnBackToMenu()
    {
        ShowPanel(PanelType.MainMenu);
    }


    // ─────────────────────────────────────────
    //  PANEL NAVIGATION
    // ─────────────────────────────────────────

    /// <summary>
    /// Shows the specified panel and hides all others.
    ///
    /// We control visibility by toggling USS classes:
    /// - AddToClassList("hidden")     → adds display: none
    /// - RemoveFromClassList("hidden") → shows the element
    ///
    /// This is exactly like element.classList.add/remove in JS!
    /// </summary>
    private enum PanelType { MainMenu, Settings, About }

    private void ShowPanel(PanelType panel)
    {
        // Hide all panels first
        _mainMenu.AddToClassList("hidden");
        _settingsPanel.AddToClassList("hidden");
        _aboutPanel.AddToClassList("hidden");

        // Show the requested panel
        switch (panel)
        {
            case PanelType.MainMenu:
                _mainMenu.RemoveFromClassList("hidden");
                break;
            case PanelType.Settings:
                _settingsPanel.RemoveFromClassList("hidden");
                break;
            case PanelType.About:
                _aboutPanel.RemoveFromClassList("hidden");
                break;
        }
    }
}
