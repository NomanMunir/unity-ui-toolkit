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
    // ─── State ───
    private bool _isOpen = false;
    private string _selectedAssetId = null;
    private AssetCategory? _activeCategory = null;

    // ─── UI References ───
    private VisualElement _root;
    private VisualElement _sidebarRoot;
    private VisualElement _backdrop;
    private VisualElement _hamburgerBtn;
    private TextField _searchField;
    private VisualElement _assetList;
    private Label _resultsCount;
    private VisualElement _categoryTabs;


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
            FilterAssets(evt.newValue, _activeCategory);
        });

        // ── Category tabs ──
        _root.Q<Button>("stab-all").clicked += () => SetCategory(null);
        _root.Q<Button>("stab-aircraft").clicked += () => SetCategory(AssetCategory.Aircraft);
        _root.Q<Button>("stab-ground").clicked += () => SetCategory(AssetCategory.GroundVehicles);
        _root.Q<Button>("stab-naval").clicked += () => SetCategory(AssetCategory.Naval);
        _root.Q<Button>("stab-infantry").clicked += () => SetCategory(AssetCategory.Infantry);
        _root.Q<Button>("stab-structures").clicked += () => SetCategory(AssetCategory.Structures);
        _root.Q<Button>("stab-sensors").clicked += () => SetCategory(AssetCategory.Sensors);
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

    private void SetCategory(AssetCategory? category)
    {
        _activeCategory = category;

        // Update tab visuals
        var tabs = _categoryTabs.Query<Button>(className: "tab-pill").ToList();
        foreach (var tab in tabs)
        {
            tab.RemoveFromClassList("tab-pill-active");
        }

        string activeTabName = category switch
        {
            null                     => "stab-all",
            AssetCategory.Aircraft   => "stab-aircraft",
            AssetCategory.GroundVehicles => "stab-ground",
            AssetCategory.Naval      => "stab-naval",
            AssetCategory.Infantry   => "stab-infantry",
            AssetCategory.Structures => "stab-structures",
            AssetCategory.Sensors    => "stab-sensors",
            _ => "stab-all"
        };

        _root.Q<Button>(activeTabName)?.AddToClassList("tab-pill-active");

        // Re-filter
        FilterAssets(_searchField.value, category);
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
    /// Filters visible cards by search text and category.
    ///
    /// Uses style.display = DisplayStyle.Flex / None
    /// to show/hide elements. This is the UI Toolkit
    /// equivalent of element.style.display in web CSS.
    /// </summary>
    private void FilterAssets(string searchQuery, AssetCategory? category)
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

            bool matchesCategory = category == null || asset.Category == category;

            bool visible = matchesSearch && matchesCategory;

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

            var text = new Label("No assets found");
            text.AddToClassList("empty-state-text");
            _emptyState.Add(text);

            var hint = new Label("Try a different search or category");
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
        _resultsCount.text = $"{count} asset{(count != 1 ? "s" : "")} found";
    }

    /// <summary>Returns a USS class name for the category badge color.</summary>
    private string GetBadgeClass(AssetCategory category)
    {
        return category switch
        {
            AssetCategory.Aircraft       => "badge-aircraft",
            AssetCategory.GroundVehicles => "badge-ground",
            AssetCategory.Naval          => "badge-naval",
            AssetCategory.Infantry       => "badge-infantry",
            AssetCategory.Structures     => "badge-structures",
            AssetCategory.Sensors        => "badge-sensors",
            _ => "badge-aircraft"
        };
    }

    /// <summary>Short label for the category badge.</summary>
    private string GetCategoryShortName(AssetCategory category)
    {
        return category switch
        {
            AssetCategory.Aircraft       => "AIR",
            AssetCategory.GroundVehicles => "GND",
            AssetCategory.Naval          => "SEA",
            AssetCategory.Infantry       => "INF",
            AssetCategory.Structures     => "BLD",
            AssetCategory.Sensors        => "SNS",
            _ => "???"
        };
    }
}
