/*
 * ============================================
 *  FormationBuilderUI.cs — Formation Builder Controller
 * ============================================
 *  Manages the formation canvas, preset buttons,
 *  and unit list for grouping assets.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the Formation Builder panel — manages unit dots
/// on the canvas, preset arrangements, and unit list.
/// </summary>
public class FormationBuilderUI
{
    private VisualElement _root;
    private VisualElement _canvasUnits;
    private TextField _formationName;
    private Label _unitCount;
    private VisualElement _unitsListContainer;

    private Formation _currentFormation;
    private Dictionary<string, VisualElement> _unitDots = new();
    private FormationType _activePreset = FormationType.VFormation;

    /// <summary>Fired when a formation is dropped to the scene.</summary>
    public event Action<Formation> OnFormationDropped;


    public void Initialize(VisualElement root)
    {
        _root = root;

        _canvasUnits = _root.Q<VisualElement>("canvas-units");
        _formationName = _root.Q<TextField>("formation-name");
        _unitCount = _root.Q<Label>("formation-unit-count");
        _unitsListContainer = _root.Q<VisualElement>("units-list-container");

        RegisterEvents();
        CreateNewFormation();
    }

    private void RegisterEvents()
    {
        // Preset buttons
        _root.Q<Button>("btn-preset-v").clicked += () => SetPreset(FormationType.VFormation, "btn-preset-v");
        _root.Q<Button>("btn-preset-line").clicked += () => SetPreset(FormationType.LineAbreast, "btn-preset-line");
        _root.Q<Button>("btn-preset-column").clicked += () => SetPreset(FormationType.Column, "btn-preset-column");
        _root.Q<Button>("btn-preset-diamond").clicked += () => SetPreset(FormationType.Diamond, "btn-preset-diamond");
        _root.Q<Button>("btn-preset-echelon").clicked += () => SetPreset(FormationType.Echelon, "btn-preset-echelon");
        _root.Q<Button>("btn-preset-custom").clicked += () => SetPreset(FormationType.Custom, "btn-preset-custom");

        // Action buttons
        _root.Q<Button>("btn-new-formation").clicked += CreateNewFormation;
        _root.Q<Button>("btn-drop-formation").clicked += DropFormation;
        _root.Q<Button>("btn-clear-formation").clicked += ClearFormation;

        // Formation name
        _formationName.RegisterValueChangedCallback(evt =>
        {
            if (_currentFormation != null)
                _currentFormation.Name = evt.newValue;
        });
    }

    // ─────────────────────────────────────────
    //  FORMATION MANAGEMENT
    // ─────────────────────────────────────────

    /// <summary>Creates a new empty formation.</summary>
    public void CreateNewFormation()
    {
        _currentFormation = new Formation("Alpha Squadron", FormationType.VFormation, Faction.BlueFOR);
        _formationName.value = _currentFormation.Name;
        _unitDots.Clear();
        _canvasUnits.Clear();
        _unitsListContainer.Clear();
        UpdateUnitCount();
    }

    /// <summary>
    /// Adds a placed asset to the current formation.
    /// Called from the Inspector's "Add to Formation" button.
    /// </summary>
    public void AddUnit(PlacedAsset placedAsset)
    {
        if (_currentFormation == null)
            CreateNewFormation();

        _currentFormation.AddUnit(placedAsset);
        _currentFormation.AssignedFaction = placedAsset.AssignedFaction;

        // Create visual dot on canvas
        CreateUnitDot(placedAsset);

        // Create entry in unit list
        CreateUnitEntry(placedAsset);

        // Update positions
        UpdateAllDotPositions();
        UpdateUnitCount();
    }

    /// <summary>Removes a unit from the formation.</summary>
    public void RemoveUnit(PlacedAsset placedAsset)
    {
        _currentFormation?.RemoveUnit(placedAsset);

        // Remove dot
        if (_unitDots.TryGetValue(placedAsset.InstanceId, out var dot))
        {
            _canvasUnits.Remove(dot);
            _unitDots.Remove(placedAsset.InstanceId);
        }

        // Rebuild unit list
        RebuildUnitList();
        UpdateAllDotPositions();
        UpdateUnitCount();
    }

    // ─────────────────────────────────────────
    //  CANVAS — Unit Dots
    // ─────────────────────────────────────────

    /// <summary>
    /// Creates a colored dot on the formation canvas.
    /// Position is set via CSS left/top as percentages.
    /// </summary>
    private void CreateUnitDot(PlacedAsset asset)
    {
        var dot = new VisualElement();
        dot.AddToClassList("unit-dot");

        // Faction color
        string factionClass = asset.AssignedFaction switch
        {
            Faction.BlueFOR => "unit-dot-blue",
            Faction.RedFOR => "unit-dot-red",
            Faction.Neutral => "unit-dot-neutral",
            _ => "unit-dot-blue"
        };
        dot.AddToClassList(factionClass);

        // Icon
        var icon = new Label(asset.Asset.Icon);
        icon.AddToClassList("unit-dot-icon");
        dot.Add(icon);

        _canvasUnits.Add(dot);
        _unitDots[asset.InstanceId] = dot;
    }

    /// <summary>
    /// Updates all dot positions on the canvas based on
    /// the formation arrangement.
    ///
    /// Converts normalized positions (-1 to 1) to
    /// percentage-based CSS left/top values.
    /// </summary>
    private void UpdateAllDotPositions()
    {
        if (_currentFormation == null) return;

        foreach (var slot in _currentFormation.Slots)
        {
            if (_unitDots.TryGetValue(slot.Asset.InstanceId, out var dot))
            {
                // Convert normalized coords to percentage
                // Center is 50%, range is roughly 10% to 90%
                float leftPct = 50f + slot.RelativePosition.x * 40f;
                float topPct = 50f - slot.RelativePosition.y * 40f; // Invert Y

                dot.style.left = new Length(leftPct, LengthUnit.Percent);
                dot.style.top = new Length(topPct, LengthUnit.Percent);

                // Leader gets special styling
                if (slot.IsLeader)
                    dot.AddToClassList("unit-dot-leader");
                else
                    dot.RemoveFromClassList("unit-dot-leader");
            }
        }
    }

    // ─────────────────────────────────────────
    //  UNIT LIST
    // ─────────────────────────────────────────

    /// <summary>Creates a unit entry in the right-side list.</summary>
    private void CreateUnitEntry(PlacedAsset asset)
    {
        var entry = new VisualElement();
        entry.AddToClassList("unit-entry");

        var icon = new Label(asset.Asset.Icon);
        icon.AddToClassList("unit-entry-icon");
        entry.Add(icon);

        var name = new Label(asset.Asset.Name);
        name.AddToClassList("unit-entry-name");
        entry.Add(name);

        // Leader badge
        var slot = _currentFormation.Slots.Find(s => s.Asset.InstanceId == asset.InstanceId);
        if (slot != null && slot.IsLeader)
        {
            var leaderBadge = new Label("★");
            leaderBadge.AddToClassList("unit-entry-leader");
            entry.Add(leaderBadge);
        }

        // Remove button
        var removeBtn = new Button(() => RemoveUnit(asset));
        removeBtn.text = "✕";
        removeBtn.AddToClassList("unit-entry-remove");
        entry.Add(removeBtn);

        _unitsListContainer.Add(entry);
    }

    /// <summary>Rebuilds the entire unit list.</summary>
    private void RebuildUnitList()
    {
        _unitsListContainer.Clear();
        if (_currentFormation == null) return;

        foreach (var slot in _currentFormation.Slots)
        {
            CreateUnitEntry(slot.Asset);
        }
    }

    // ─────────────────────────────────────────
    //  PRESETS
    // ─────────────────────────────────────────

    private void SetPreset(FormationType type, string buttonName)
    {
        _activePreset = type;
        if (_currentFormation != null)
        {
            _currentFormation.Type = type;
            _currentFormation.ArrangeSlots();
            UpdateAllDotPositions();
        }

        // Update active button styling
        var presetButtons = _root.Query<Button>(className: "preset-btn").ToList();
        foreach (var btn in presetButtons)
        {
            btn.RemoveFromClassList("preset-btn-active");
        }
        _root.Q<Button>(buttonName)?.AddToClassList("preset-btn-active");
    }

    // ─────────────────────────────────────────
    //  ACTIONS
    // ─────────────────────────────────────────

    private void DropFormation()
    {
        if (_currentFormation == null || _currentFormation.Slots.Count == 0)
        {
            Debug.Log("No units in formation to drop.");
            return;
        }

        _currentFormation.Name = _formationName.value;
        OnFormationDropped?.Invoke(_currentFormation);
        Debug.Log($"Formation '{_currentFormation.Name}' dropped to scene with {_currentFormation.Slots.Count} units.");

        // Create a new formation for next use
        CreateNewFormation();
    }

    private void ClearFormation()
    {
        _currentFormation?.Slots.Clear();
        _unitDots.Clear();
        _canvasUnits.Clear();
        _unitsListContainer.Clear();
        UpdateUnitCount();
    }

    private void UpdateUnitCount()
    {
        int count = _currentFormation?.Slots.Count ?? 0;
        _unitCount.text = $"{count} unit{(count != 1 ? "s" : "")}";
    }
}
