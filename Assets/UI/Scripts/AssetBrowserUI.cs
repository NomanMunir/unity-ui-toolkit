/*
 * ============================================
 *  AssetBrowserUI.cs — Asset Browser Controller
 * ============================================
 *  Populates the tile grid dynamically from AssetDatabase,
 *  handles search/filter, and fires selection events.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the Asset Browser panel — populates tiles,
/// handles search, category filtering, and selection.
/// </summary>
public class AssetBrowserUI
{
    private VisualElement _root;
    private TextField _searchField;
    private VisualElement _categoryTabs;
    private Label _assetCountLabel;

    private string _selectedAssetId;
    private AssetCategory? _activeFilter = null;

    // Grids for each category
    private Dictionary<AssetCategory, VisualElement> _grids = new();
    private Dictionary<AssetCategory, Foldout> _foldouts = new();

    /// <summary>
    /// Fired when the user clicks an asset tile.
    /// Passes the selected SimulationAsset.
    /// </summary>
    public event Action<SimulationAsset> OnAssetSelected;

    // ─────────────────────────────────────────

    /// <summary>
    /// Initializes the Asset Browser with a root visual element.
    /// Call this from the master controller's OnEnable.
    /// </summary>
    public void Initialize(VisualElement root)
    {
        _root = root;

        // Cache references
        _searchField = _root.Q<TextField>("search-field");
        _assetCountLabel = _root.Q<Label>("asset-count");
        _categoryTabs = _root.Q<VisualElement>("category-tabs");

        // Cache grids and foldouts
        _grids[AssetCategory.Aircraft] = _root.Q<VisualElement>("grid-aircraft");
        _grids[AssetCategory.GroundVehicles] = _root.Q<VisualElement>("grid-ground");
        _grids[AssetCategory.Naval] = _root.Q<VisualElement>("grid-naval");
        _grids[AssetCategory.Infantry] = _root.Q<VisualElement>("grid-infantry");
        _grids[AssetCategory.Structures] = _root.Q<VisualElement>("grid-structures");
        _grids[AssetCategory.Sensors] = _root.Q<VisualElement>("grid-sensors");

        _foldouts[AssetCategory.Aircraft] = _root.Q<Foldout>("foldout-aircraft");
        _foldouts[AssetCategory.GroundVehicles] = _root.Q<Foldout>("foldout-ground");
        _foldouts[AssetCategory.Naval] = _root.Q<Foldout>("foldout-naval");
        _foldouts[AssetCategory.Infantry] = _root.Q<Foldout>("foldout-infantry");
        _foldouts[AssetCategory.Structures] = _root.Q<Foldout>("foldout-structures");
        _foldouts[AssetCategory.Sensors] = _root.Q<Foldout>("foldout-sensors");

        // Register events
        RegisterEvents();

        // Populate initial tiles
        PopulateAllTiles();
    }

    // ─────────────────────────────────────────

    private void RegisterEvents()
    {
        // Search field
        _searchField.RegisterValueChangedCallback(evt => FilterAssets(evt.newValue));

        // Category tabs
        _root.Q<Button>("tab-all").clicked += () => SetCategoryFilter(null);
        _root.Q<Button>("tab-aircraft").clicked += () => SetCategoryFilter(AssetCategory.Aircraft);
        _root.Q<Button>("tab-ground").clicked += () => SetCategoryFilter(AssetCategory.GroundVehicles);
        _root.Q<Button>("tab-naval").clicked += () => SetCategoryFilter(AssetCategory.Naval);
        _root.Q<Button>("tab-infantry").clicked += () => SetCategoryFilter(AssetCategory.Infantry);
        _root.Q<Button>("tab-structures").clicked += () => SetCategoryFilter(AssetCategory.Structures);
        _root.Q<Button>("tab-sensors").clicked += () => SetCategoryFilter(AssetCategory.Sensors);
    }

    // ─────────────────────────────────────────
    //  TILE CREATION
    // ─────────────────────────────────────────

    /// <summary>
    /// Populates all category grids with asset tiles.
    /// This demonstrates DYNAMIC UI CREATION —
    /// creating VisualElements in C# instead of UXML.
    /// </summary>
    private void PopulateAllTiles()
    {
        var allAssets = SimAssetDatabase.GetAllAssets();

        foreach (var category in _grids.Keys)
        {
            var grid = _grids[category];
            grid.Clear();

            var categoryAssets = allAssets.Where(a => a.Category == category).ToList();

            foreach (var asset in categoryAssets)
            {
                var tile = CreateAssetTile(asset);
                grid.Add(tile);
            }
        }

        UpdateAssetCount(allAssets.Count);
    }

    /// <summary>
    /// Creates a single asset tile element.
    ///
    /// DYNAMIC UI CREATION:
    /// - new VisualElement() → creates a div
    /// - new Label() → creates text
    /// - AddToClassList() → adds USS styling
    /// - Add() → appends child element
    ///
    /// This is like document.createElement() + appendChild() in JS.
    /// </summary>
    private VisualElement CreateAssetTile(SimulationAsset asset)
    {
        // Container
        var tile = new VisualElement();
        tile.name = $"tile-{asset.Id}";
        tile.AddToClassList("asset-tile");

        // Store the asset ID as user data for later retrieval
        tile.userData = asset.Id;

        // Icon label
        var icon = new Label(asset.Icon);
        icon.AddToClassList("asset-tile-icon");
        tile.Add(icon);

        // Name label
        var name = new Label(asset.Name);
        name.AddToClassList("asset-tile-name");
        tile.Add(name);

        // Click handler
        tile.RegisterCallback<ClickEvent>(evt =>
        {
            SelectAsset(asset.Id);
        });

        return tile;
    }

    // ─────────────────────────────────────────
    //  SELECTION
    // ─────────────────────────────────────────

    /// <summary>
    /// Selects an asset and highlights the tile.
    /// </summary>
    private void SelectAsset(string assetId)
    {
        // Deselect previous
        if (_selectedAssetId != null)
        {
            var prevTile = _root.Q<VisualElement>($"tile-{_selectedAssetId}");
            prevTile?.RemoveFromClassList("asset-tile-selected");
        }

        // Select new
        _selectedAssetId = assetId;
        var tile = _root.Q<VisualElement>($"tile-{assetId}");
        tile?.AddToClassList("asset-tile-selected");

        // Fire event
        var asset = SimAssetDatabase.GetById(assetId);
        if (asset != null)
        {
            OnAssetSelected?.Invoke(asset);
        }
    }

    // ─────────────────────────────────────────
    //  FILTERING
    // ─────────────────────────────────────────

    /// <summary>
    /// Filters assets by search query.
    /// Shows/hides tiles and foldouts based on matches.
    /// </summary>
    private void FilterAssets(string query)
    {
        var results = SimAssetDatabase.Search(query);
        int visibleCount = 0;

        foreach (var category in _grids.Keys)
        {
            var grid = _grids[category];
            var foldout = _foldouts[category];
            bool hasVisibleTiles = false;

            foreach (var child in grid.Children())
            {
                string assetId = child.userData as string;
                bool matches = results.Exists(a => a.Id == assetId);

                child.style.display = matches
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (matches)
                {
                    hasVisibleTiles = true;
                    visibleCount++;
                }
            }

            // Hide entire category if no matches
            foldout.style.display = hasVisibleTiles
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        UpdateAssetCount(visibleCount);
    }

    /// <summary>
    /// Filters by category tab.
    /// </summary>
    private void SetCategoryFilter(AssetCategory? category)
    {
        _activeFilter = category;

        // Update tab active states
        foreach (var tab in _categoryTabs.Children())
        {
            tab.RemoveFromClassList("category-tab-active");
        }

        string activeTabName = category switch
        {
            null => "tab-all",
            AssetCategory.Aircraft => "tab-aircraft",
            AssetCategory.GroundVehicles => "tab-ground",
            AssetCategory.Naval => "tab-naval",
            AssetCategory.Infantry => "tab-infantry",
            AssetCategory.Structures => "tab-structures",
            AssetCategory.Sensors => "tab-sensors",
            _ => "tab-all"
        };

        _root.Q<Button>(activeTabName)?.AddToClassList("category-tab-active");

        // Show/hide foldouts
        foreach (var kvp in _foldouts)
        {
            bool show = category == null || kvp.Key == category;
            kvp.Value.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void UpdateAssetCount(int count)
    {
        if (_assetCountLabel != null)
            _assetCountLabel.text = $"{count} asset{(count != 1 ? "s" : "")}";
    }
}
