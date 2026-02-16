/*
 * ============================================
 *  SidebarMenuController.cs — Hamburger Sidebar Controller
 * ============================================
 *  Attach to a GameObject with a UIDocument.
 *  Set the Source Asset to SidebarMenu.uxml.
 *
 *  UI TOOLKIT FEATURES DEMONSTRATED:
 *
 *  1. Q<T>() and Query<T>()  — Finding elements by name/class
 *  2. AddToClassList / RemoveFromClassList — State-driven animation
 *  3. RegisterCallback<T>()  — Click, hover, change events
 *  4. new VisualElement()    — Dynamic element creation
 *  5. ToggleInClassList()    — Toggle a USS class (single call)
 *  6. schedule.Execute()     — Delayed/timed operations
 *  7. RegisterValueChangedCallback — Live text input handling
 *  8. style.display          — Show/hide elements
 *  9. userData               — Attach custom data to elements
 * 10. ScrollView             — Scrollable containers
 * 11. THEMING                — CSS variable override via class toggle
 * 12. LOCALIZATION           — Data binding pattern for multi-language
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the hamburger sidebar menu.
/// Handles open/close, search, category filtering, and asset card creation.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class SidebarMenuController : MonoBehaviour
{
    // ─── Inspector Fields ───
    // Drag your TSS assets here in the Unity Inspector
    [Header("Theme Style Sheets (TSS)")]
    [Tooltip("Assign DarkTheme.tss from Themes/ folder")]
    [SerializeField] private ThemeStyleSheet _darkThemeTSS;

    [Tooltip("Assign LightTheme.tss from Themes/ folder")]
    [SerializeField] private ThemeStyleSheet _lightThemeTSS;
    // ─── State ───
    private bool _isOpen = false;
    private string _selectedAssetId = null;
    private bool _isDarkTheme = true;

    // Simplified 3-group filter: null=All, "air", "land", "sea"
    private string _activeGroup = null;

    // ─── UI References ───
    private VisualElement _root;
    private VisualElement _sidebarRoot;
    private VisualElement _backdrop;
    private VisualElement _hamburgerBtn;
    private TextField _searchField;
    private VisualElement _assetList;
    private Label _resultsCount;
    private VisualElement _categoryTabs;

    // Theme & Language
    private Button _themeBtn;
    private Button _langBtn;
    private Label _headerTitle;
    private Label _headerSubtitle;
    private Label _footerText;


    // ─────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        CacheReferences();
        RegisterEvents();
        PopulateAssets();

        Debug.Log("[SidebarMenu] Initialized. Click the hamburger to open.");
    }

    /// <summary>
    /// Q<T>("name") finds an element by its name attribute.
    /// This is the UI Toolkit equivalent of document.getElementById().
    /// </summary>
    private void CacheReferences()
    {
        _sidebarRoot   = _root.Q<VisualElement>("sidebar-root");
        _backdrop      = _root.Q<VisualElement>("backdrop");
        _hamburgerBtn  = _root.Q<VisualElement>("hamburger-btn");
        _searchField   = _root.Q<TextField>("sidebar-search");
        _assetList     = _root.Q<VisualElement>("asset-list");
        _resultsCount  = _root.Q<Label>("results-count");
        _categoryTabs  = _root.Q<VisualElement>("category-tabs");

        // Theme & Language buttons
        _themeBtn       = _root.Q<Button>("btn-theme");
        _langBtn        = _root.Q<Button>("btn-lang");
        _headerTitle    = _root.Q<Label>("lbl-header-title");
        _headerSubtitle = _root.Q<Label>("lbl-header-subtitle");
        _footerText     = _root.Q<Label>("lbl-footer");
    }


    // ─────────────────────────────────────────
    //  EVENT REGISTRATION
    // ─────────────────────────────────────────

    /// <summary>
    /// RegisterCallback<ClickEvent> = addEventListener("click", handler)
    /// RegisterValueChangedCallback = addEventListener("input", handler)
    ///
    /// UI Toolkit uses a strongly-typed event system. Each event
    /// (ClickEvent, PointerEnterEvent, etc.) is a separate class.
    /// </summary>
    private void RegisterEvents()
    {
        // ── Hamburger toggle ──
        _hamburgerBtn.RegisterCallback<ClickEvent>(evt =>
        {
            ToggleSidebar();
            evt.StopPropagation(); // Prevent click from reaching backdrop
        });

        // ── Backdrop click closes sidebar ──
        _backdrop.RegisterCallback<ClickEvent>(evt =>
        {
            if (_isOpen) CloseSidebar();
        });

        // ── Search field — live filtering ──
        _searchField.RegisterValueChangedCallback(evt =>
        {
            FilterAssets(evt.newValue, _activeGroup);
        });

        // ── Category tabs (3 groups: Air, Land, Sea) ──
        _root.Q<Button>("stab-all").clicked += () => SetGroup(null);
        _root.Q<Button>("stab-air").clicked += () => SetGroup("air");
        _root.Q<Button>("stab-land").clicked += () => SetGroup("land");
        _root.Q<Button>("stab-sea").clicked += () => SetGroup("sea");

        // ── Theme toggle ──
        _themeBtn.clicked += ToggleTheme;

        // ── Language cycle ──
        _langBtn.clicked += CycleLanguage;
    }


    // ─────────────────────────────────────────
    //  SIDEBAR OPEN / CLOSE
    // ─────────────────────────────────────────

    /// <summary>
    /// ToggleInClassList("class") adds the class if absent,
    /// removes it if present. One call, no if-else needed.
    ///
    /// All animations happen purely in USS via transitions —
    /// no C# animation code needed! Just toggle the class and
    /// USS handles the rest.
    /// </summary>
    private void ToggleSidebar()
    {
        if (_isOpen)
            CloseSidebar();
        else
            OpenSidebar();
    }

    private void OpenSidebar()
    {
        _isOpen = true;
        _sidebarRoot.AddToClassList("sidebar-open");

        // Make backdrop interactive
        _backdrop.pickingMode = PickingMode.Position;

        Debug.Log("[SidebarMenu] Opened");
    }

    private void CloseSidebar()
    {
        _isOpen = false;
        _sidebarRoot.RemoveFromClassList("sidebar-open");

        // Remove backdrop interaction so clicks pass through
        _backdrop.pickingMode = PickingMode.Ignore;

        Debug.Log("[SidebarMenu] Closed");
    }


    // ─────────────────────────────────────────
    //  CATEGORY FILTERING
    // ─────────────────────────────────────────

    private void SetGroup(string group)
    {
        _activeGroup = group;

        // Update tab visuals
        var tabs = _categoryTabs.Query<Button>(className: "tab-pill").ToList();
        foreach (var tab in tabs)
        {
            tab.RemoveFromClassList("tab-pill-active");
        }

        string activeTabName = group switch
        {
            "air"  => "stab-air",
            "land" => "stab-land",
            "sea"  => "stab-sea",
            _      => "stab-all"
        };

        _root.Q<Button>(activeTabName)?.AddToClassList("tab-pill-active");

        // Re-filter
        FilterAssets(_searchField.value, group);
    }

    /// <summary>Maps AssetCategory to our 3 groups.</summary>
    private string CategoryToGroup(AssetCategory cat)
    {
        return cat switch
        {
            AssetCategory.Aircraft       => "air",
            AssetCategory.Naval          => "sea",
            // Everything else = land
            _ => "land"
        };
    }


    // ─────────────────────────────────────────
    //  THEME TOGGLE
    // ─────────────────────────────────────────

    /// <summary>
    /// Toggles between dark and light themes.
    ///
    /// TWO APPROACHES are demonstrated here:
    ///
    /// 1. TSS SWAP (Preferred / Unity-native)
    ///    panelSettings.themeStyleSheet = lightThemeTSS;
    ///    - Swaps the entire Theme Style Sheet at the PanelSettings level
    ///    - Affects ALL UIDocuments using that PanelSettings
    ///    - Requires .tss assets assigned in Inspector
    ///    - This is the PROPER Unity way to do theming
    ///
    /// 2. CLASS TOGGLE (Fallback / CSS-like)
    ///    root.ToggleInClassList("theme-light");
    ///    - Overrides CSS variables via USS class
    ///    - Only affects this specific UIDocument
    ///    - Works without any Inspector setup
    ///
    /// The code tries TSS first, falls back to class toggle.
    /// </summary>
    private void ToggleTheme()
    {
        _isDarkTheme = !_isDarkTheme;

        // ── Approach 1: TSS swap via PanelSettings (preferred) ──
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc.panelSettings != null && _darkThemeTSS != null && _lightThemeTSS != null)
        {
            uiDoc.panelSettings.themeStyleSheet = _isDarkTheme ? _darkThemeTSS : _lightThemeTSS;
            Debug.Log($"[SidebarMenu] Theme via TSS: {(_isDarkTheme ? "Dark" : "Light")}");
        }
        else
        {
            // ── Approach 2: USS class toggle fallback ──
            _sidebarRoot.ToggleInClassList("theme-light");
            Debug.Log($"[SidebarMenu] Theme via class toggle: {(_isDarkTheme ? "Dark" : "Light")}");
        }

        // Update button label
        _themeBtn.text = _isDarkTheme ? "Dark" : "Light";
    }


    // ─────────────────────────────────────────
    //  LANGUAGE / LOCALIZATION
    // ─────────────────────────────────────────

    /// <summary>
    /// Cycles through available languages: EN → AR → FR → EN...
    ///
    /// DATA BINDING PATTERN:
    ///   1. Each UI label is associated with a localization KEY
    ///   2. When language changes, we call LocalizationData.Get(key)
    ///   3. Each label's .text is updated with the translated string
    ///   4. This is a "manual binding" approach — simple and explicit
    ///
    /// In production Unity, you'd use the Localization package which
    /// provides automatic binding via LocalizedString + StringTable.
    /// </summary>
    private void CycleLanguage()
    {
        int idx = LocalizationData.GetCurrentLanguageIndex();
        int next = (idx + 1) % LocalizationData.SupportedLanguages.Length;

        string newLang = LocalizationData.SupportedLanguages[next];
        LocalizationData.SetLanguage(newLang);

        // Update the button label to show current language
        _langBtn.text = newLang.ToUpper();

        // Re-bind all localized labels
        ApplyLocalization();

        Debug.Log($"[SidebarMenu] Language: {LocalizationData.LanguageNames[next]} ({newLang})");
    }

    /// <summary>
    /// Re-applies all localized strings to their labels.
    ///
    /// BINDING MAP:
    ///   Label name         →  Localization key
    ///   lbl-header-title   →  "header.title"
    ///   lbl-header-subtitle→  "header.subtitle"
    ///   lbl-footer         →  "footer.text"
    ///   stab-all           →  "tab.all"
    ///   results-count      →  "results.count"
    /// </summary>
    private void ApplyLocalization()
    {
        // ── Header ──
        _headerTitle.text    = LocalizationData.Get("header.title");
        _headerSubtitle.text = LocalizationData.Get("header.subtitle");

        // ── Tabs ──
        _root.Q<Button>("stab-all").text  = LocalizationData.Get("tab.all");
        _root.Q<Button>("stab-air").text  = LocalizationData.Get("tab.air");
        _root.Q<Button>("stab-land").text = LocalizationData.Get("tab.land");
        _root.Q<Button>("stab-sea").text  = LocalizationData.Get("tab.sea");

        // ── Footer ──
        _footerText.text = LocalizationData.Get("footer.text");

        // ── Re-count with localized format ──
        int visibleCount = 0;
        foreach (var child in _assetList.Children())
        {
            if (child.userData is SimulationAsset &&
                child.style.display != DisplayStyle.None)
                visibleCount++;
        }
        UpdateResultsCount(visibleCount);

        // ── Update empty state if visible ──
        if (_emptyState != null)
        {
            _emptyState.Q<Label>(className: "empty-state-text").text = LocalizationData.Get("empty.title");
            _emptyState.Q<Label>(className: "empty-state-hint").text = LocalizationData.Get("empty.hint");
        }
    }


    // ─────────────────────────────────────────
    //  POPULATE ASSETS — Dynamic Element Creation
    // ─────────────────────────────────────────

    /// <summary>
    /// Creates a card VisualElement for each asset.
    ///
    /// DYNAMIC UI CREATION works like the DOM API:
    ///   var div = new VisualElement();     // document.createElement("div")
    ///   div.AddToClassList("my-class");    // div.classList.add("my-class")
    ///   parent.Add(div);                   // parent.appendChild(div)
    ///   div.userData = myData;             // div.dataset.myData = myData
    /// </summary>
    private void PopulateAssets()
    {
        _assetList.Clear();

        var allAssets = SimAssetDatabase.GetAllAssets();

        foreach (var asset in allAssets)
        {
            var card = CreateAssetCard(asset);
            _assetList.Add(card);
        }

        UpdateResultsCount(allAssets.Count);
    }

    /// <summary>
    /// Builds a single asset card from data.
    /// Each card has: icon, name, description, category badge.
    /// </summary>
    private VisualElement CreateAssetCard(SimulationAsset asset)
    {
        // ── Card container ──
        var card = new VisualElement();
        card.name = $"card-{asset.Id}";
        card.AddToClassList("asset-card");
        card.userData = asset;

        // ── Icon wrapper ──
        var iconWrap = new VisualElement();
        iconWrap.AddToClassList("card-icon-wrap");

        var icon = new Label(asset.Icon);
        icon.AddToClassList("card-icon");
        iconWrap.Add(icon);
        card.Add(iconWrap);

        // ── Text area ──
        var textArea = new VisualElement();
        textArea.AddToClassList("card-text-area");

        var nameLabel = new Label(asset.Name);
        nameLabel.AddToClassList("card-name");
        textArea.Add(nameLabel);

        var descLabel = new Label(asset.Description);
        descLabel.AddToClassList("card-desc");
        textArea.Add(descLabel);

        card.Add(textArea);

        // ── Category badge ──
        var badge = new VisualElement();
        badge.AddToClassList("card-badge");
        badge.AddToClassList(GetBadgeClass(asset.Category));

        var badgeText = new Label(GetCategoryShortName(asset.Category));
        badgeText.AddToClassList("card-badge-text");
        badge.Add(badgeText);

        card.Add(badge);

        // ── Click handler ──
        card.RegisterCallback<ClickEvent>(evt =>
        {
            SelectAsset(asset);
            evt.StopPropagation();
        });

        return card;
    }


    // ─────────────────────────────────────────
    //  ASSET SELECTION
    // ─────────────────────────────────────────

    /// <summary>
    /// Selects an asset card. Uses AddToClassList/RemoveFromClassList
    /// to toggle the selected visual state, which triggers the
    /// USS transition for a smooth highlight animation.
    /// </summary>
    private void SelectAsset(SimulationAsset asset)
    {
        // Deselect previous
        if (_selectedAssetId != null)
        {
            var prevCard = _root.Q<VisualElement>($"card-{_selectedAssetId}");
            prevCard?.RemoveFromClassList("asset-card-selected");
        }

        // Select new
        _selectedAssetId = asset.Id;
        var card = _root.Q<VisualElement>($"card-{asset.Id}");
        card?.AddToClassList("asset-card-selected");

        Debug.Log($"[SidebarMenu] Selected: {asset.Icon} {asset.Name} — {asset.Description}");
    }


    // ─────────────────────────────────────────
    //  FILTERING & SEARCH
    // ─────────────────────────────────────────

    /// <summary>
    /// Filters visible cards by search text and group (air/land/sea).
    ///
    /// Uses style.display = DisplayStyle.Flex / None
    /// to show/hide elements. This is the UI Toolkit
    /// equivalent of element.style.display in web CSS.
    /// </summary>
    private void FilterAssets(string searchQuery, string group)
    {
        int visibleCount = 0;
        string query = searchQuery?.ToLower() ?? "";

        foreach (var child in _assetList.Children())
        {
            var asset = child.userData as SimulationAsset;
            if (asset == null) continue;

            bool matchesSearch = string.IsNullOrEmpty(query)
                || asset.Name.ToLower().Contains(query)
                || asset.Description.ToLower().Contains(query);

            bool matchesGroup = group == null || CategoryToGroup(asset.Category) == group;

            bool visible = matchesSearch && matchesGroup;

            child.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

            if (visible) visibleCount++;
        }

        UpdateResultsCount(visibleCount);

        // Show empty state if no results
        ShowEmptyState(visibleCount == 0);
    }


    // ─────────────────────────────────────────
    //  EMPTY STATE
    // ─────────────────────────────────────────

    private VisualElement _emptyState;

    /// <summary>
    /// Dynamically creates/removes an empty state message.
    /// Shows when search or filter returns zero results.
    /// </summary>
    private void ShowEmptyState(bool show)
    {
        if (show && _emptyState == null)
        {
            _emptyState = new VisualElement();
            _emptyState.AddToClassList("empty-state");

            var icon = new Label("🔎");
            icon.AddToClassList("empty-state-icon");
            _emptyState.Add(icon);

            var text = new Label(LocalizationData.Get("empty.title"));
            text.AddToClassList("empty-state-text");
            _emptyState.Add(text);

            var hint = new Label(LocalizationData.Get("empty.hint"));
            hint.AddToClassList("empty-state-hint");
            _emptyState.Add(hint);

            _assetList.Add(_emptyState);
        }
        else if (!show && _emptyState != null)
        {
            _assetList.Remove(_emptyState);
            _emptyState = null;
        }
    }


    // ─────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────

    private void UpdateResultsCount(int count)
    {
        _resultsCount.text = LocalizationData.Get("results.count", count);
    }

    /// <summary>Returns a USS class name for the group badge color.</summary>
    private string GetBadgeClass(AssetCategory category)
    {
        string group = CategoryToGroup(category);
        return group switch
        {
            "air"  => "badge-air",
            "land" => "badge-land",
            "sea"  => "badge-sea",
            _ => "badge-air"
        };
    }

    /// <summary>Short label for the group badge.</summary>
    private string GetCategoryShortName(AssetCategory category)
    {
        string group = CategoryToGroup(category);
        return group switch
        {
            "air"  => "AIR",
            "land" => "LAND",
            "sea"  => "SEA",
            _ => "???"
        };
    }
}
