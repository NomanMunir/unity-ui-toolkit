/*
 * ============================================
 *  InspectorPanelUI.cs — Inspector Controller
 * ============================================
 *  Displays selected asset properties and handles
 *  faction selection, quantity, and add-to-scene/formation.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the Inspector Panel — shows asset details,
/// generates property controls dynamically, handles actions.
/// </summary>
public class InspectorPanelUI
{
    private VisualElement _root;
    private VisualElement _emptyState;
    private ScrollView _content;

    // Asset info
    private Label _previewIcon;
    private Label _assetName;
    private Label _assetDescription;
    private Label _categoryLabel;

    // Faction
    private Button _btnBlue, _btnRed, _btnNeutral;
    private Faction _selectedFaction = Faction.BlueFOR;

    // Properties
    private VisualElement _propertiesContainer;
    private SimulationAsset _currentAsset;

    // Quantity
    private Label _quantityLabel;
    private int _quantity = 1;

    /// <summary>Fired when "Add to Scene" is clicked.</summary>
    public event Action<SimulationAsset, Faction, int> OnAddToScene;

    /// <summary>Fired when "Add to Formation" is clicked.</summary>
    public event Action<SimulationAsset, Faction, int> OnAddToFormation;


    public void Initialize(VisualElement root)
    {
        _root = root;

        _emptyState = _root.Q<VisualElement>("inspector-empty");
        _content = _root.Q<ScrollView>("inspector-content");

        _previewIcon = _root.Q<Label>("preview-icon");
        _assetName = _root.Q<Label>("asset-name");
        _assetDescription = _root.Q<Label>("asset-description");
        _categoryLabel = _root.Q<Label>("asset-category-label");

        _btnBlue = _root.Q<Button>("btn-faction-blue");
        _btnRed = _root.Q<Button>("btn-faction-red");
        _btnNeutral = _root.Q<Button>("btn-faction-neutral");

        _propertiesContainer = _root.Q<VisualElement>("properties-container");
        _quantityLabel = _root.Q<Label>("quantity-label");

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        // Faction buttons
        _btnBlue.clicked += () => SetFaction(Faction.BlueFOR);
        _btnRed.clicked += () => SetFaction(Faction.RedFOR);
        _btnNeutral.clicked += () => SetFaction(Faction.Neutral);

        // Quantity buttons
        _root.Q<Button>("btn-qty-minus").clicked += () => SetQuantity(_quantity - 1);
        _root.Q<Button>("btn-qty-plus").clicked += () => SetQuantity(_quantity + 1);

        // Action buttons
        _root.Q<Button>("btn-add-scene").clicked += () =>
        {
            if (_currentAsset != null)
                OnAddToScene?.Invoke(_currentAsset, _selectedFaction, _quantity);
        };

        _root.Q<Button>("btn-add-formation").clicked += () =>
        {
            if (_currentAsset != null)
                OnAddToFormation?.Invoke(_currentAsset, _selectedFaction, _quantity);
        };
    }

    // ─────────────────────────────────────────
    //  DISPLAY ASSET
    // ─────────────────────────────────────────

    /// <summary>
    /// Shows asset details in the inspector.
    /// Called when an asset is selected in the browser.
    /// </summary>
    public void ShowAsset(SimulationAsset asset)
    {
        _currentAsset = asset;
        _quantity = 1;
        _quantityLabel.text = "1";

        // Switch from empty state to content
        _emptyState.AddToClassList("hidden");
        _content.RemoveFromClassList("hidden");

        // Update info
        _previewIcon.text = asset.Icon;
        _assetName.text = asset.Name;
        _assetDescription.text = asset.Description;
        _categoryLabel.text = asset.Category.ToString();

        // Generate property controls
        GeneratePropertyControls(asset.Properties);
    }

    /// <summary>
    /// Shows the empty state (no asset selected).
    /// </summary>
    public void ClearSelection()
    {
        _currentAsset = null;
        _emptyState.RemoveFromClassList("hidden");
        _content.AddToClassList("hidden");
    }

    // ─────────────────────────────────────────
    //  DYNAMIC PROPERTY GENERATION
    // ─────────────────────────────────────────

    /// <summary>
    /// Generates UI controls for each asset property.
    /// This is a key UI Toolkit pattern — building complex
    /// UI dynamically based on data.
    /// </summary>
    private void GeneratePropertyControls(List<AssetProperty> properties)
    {
        _propertiesContainer.Clear();

        foreach (var prop in properties)
        {
            var row = new VisualElement();
            row.AddToClassList("property-row");

            // Label
            var label = new Label(prop.Name);
            label.AddToClassList("property-label");
            row.Add(label);

            // Control area
            var controlArea = new VisualElement();
            controlArea.AddToClassList("property-control");

            switch (prop.Type)
            {
                case AssetProperty.PropertyType.Float:
                    CreateFloatControl(controlArea, prop);
                    break;

                case AssetProperty.PropertyType.Int:
                    CreateIntControl(controlArea, prop);
                    break;

                case AssetProperty.PropertyType.Bool:
                    CreateBoolControl(controlArea, prop);
                    break;

                case AssetProperty.PropertyType.Dropdown:
                    CreateDropdownControl(controlArea, prop);
                    break;
            }

            row.Add(controlArea);
            _propertiesContainer.Add(row);
        }
    }

    private void CreateFloatControl(VisualElement container, AssetProperty prop)
    {
        var slider = new Slider(prop.MinValue, prop.MaxValue);
        slider.value = prop.FloatValue;
        slider.AddToClassList("property-slider");

        var valueLabel = new Label($"{prop.FloatValue:F0}{prop.Unit}");
        valueLabel.AddToClassList("property-value");

        slider.RegisterValueChangedCallback(evt =>
        {
            prop.FloatValue = evt.newValue;
            valueLabel.text = $"{evt.newValue:F0}{prop.Unit}";
        });

        container.Add(slider);
        container.Add(valueLabel);
    }

    private void CreateIntControl(VisualElement container, AssetProperty prop)
    {
        var slider = new SliderInt((int)prop.MinValue, (int)prop.MaxValue);
        slider.value = prop.IntValue;
        slider.AddToClassList("property-slider");

        var valueLabel = new Label($"{prop.IntValue}{prop.Unit}");
        valueLabel.AddToClassList("property-value");

        slider.RegisterValueChangedCallback(evt =>
        {
            prop.IntValue = evt.newValue;
            valueLabel.text = $"{evt.newValue}{prop.Unit}";
        });

        container.Add(slider);
        container.Add(valueLabel);
    }

    private void CreateBoolControl(VisualElement container, AssetProperty prop)
    {
        var toggle = new Toggle();
        toggle.value = prop.BoolValue;
        toggle.AddToClassList("property-toggle");

        toggle.RegisterValueChangedCallback(evt =>
        {
            prop.BoolValue = evt.newValue;
        });

        container.Add(toggle);
    }

    private void CreateDropdownControl(VisualElement container, AssetProperty prop)
    {
        var dropdown = new DropdownField(prop.DropdownOptions, prop.DropdownIndex);
        dropdown.AddToClassList("property-dropdown");

        dropdown.RegisterValueChangedCallback(evt =>
        {
            prop.DropdownIndex = prop.DropdownOptions.IndexOf(evt.newValue);
        });

        container.Add(dropdown);
    }

    // ─────────────────────────────────────────
    //  FACTION SELECTION
    // ─────────────────────────────────────────

    private void SetFaction(Faction faction)
    {
        _selectedFaction = faction;

        // Update visual states
        _btnBlue.RemoveFromClassList("faction-btn-active");
        _btnRed.RemoveFromClassList("faction-btn-active");
        _btnNeutral.RemoveFromClassList("faction-btn-active");

        switch (faction)
        {
            case Faction.BlueFOR:  _btnBlue.AddToClassList("faction-btn-active"); break;
            case Faction.RedFOR:   _btnRed.AddToClassList("faction-btn-active"); break;
            case Faction.Neutral:  _btnNeutral.AddToClassList("faction-btn-active"); break;
        }
    }

    // ─────────────────────────────────────────
    //  QUANTITY
    // ─────────────────────────────────────────

    private void SetQuantity(int qty)
    {
        _quantity = Mathf.Clamp(qty, 1, 20);
        _quantityLabel.text = _quantity.ToString();
    }
}
