/*
 * ============================================
 *  SceneHierarchyUI.cs — Scene Hierarchy Controller
 * ============================================
 *  Manages the tree view of placed entities,
 *  grouped by faction with formation support.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the Scene Hierarchy panel — tracks all placed
/// entities, groups by faction, displays formations.
/// </summary>
public class SceneHierarchyUI
{
    private VisualElement _root;
    private VisualElement _blueList;
    private VisualElement _redList;
    private VisualElement _neutralList;
    private Label _entityCount;

    // Track all placed assets
    private List<PlacedAsset> _placedAssets = new();
    private List<Formation> _formations = new();

    /// <summary>Fired when a hierarchy entry is clicked.</summary>
    public event Action<PlacedAsset> OnEntitySelected;

    /// <summary>Fired when all entities are cleared.</summary>
    public event Action OnSceneCleared;


    public void Initialize(VisualElement root)
    {
        _root = root;

        _blueList = _root.Q<VisualElement>("blue-force-list");
        _redList = _root.Q<VisualElement>("red-force-list");
        _neutralList = _root.Q<VisualElement>("neutral-force-list");
        _entityCount = _root.Q<Label>("entity-count");

        _root.Q<Button>("btn-clear-scene").clicked += ClearAll;
    }

    // ─────────────────────────────────────────
    //  ADD ENTITIES
    // ─────────────────────────────────────────

    /// <summary>
    /// Adds a single asset to the scene hierarchy.
    /// </summary>
    public void AddEntity(PlacedAsset asset)
    {
        _placedAssets.Add(asset);
        var list = GetFactionList(asset.AssignedFaction);
        ClearEmptyLabel(list);

        var entry = CreateHierarchyEntry(asset);
        list.Add(entry);
        UpdateEntityCount();
    }

    /// <summary>
    /// Adds an entire formation to the scene hierarchy.
    /// Creates a collapsible group with child entries.
    /// </summary>
    public void AddFormation(Formation formation)
    {
        _formations.Add(formation);
        var list = GetFactionList(formation.AssignedFaction);
        ClearEmptyLabel(list);

        // Formation group container
        var group = new VisualElement();
        group.AddToClassList("formation-group");
        group.name = $"formation-{formation.Id}";

        // Group header
        var header = new VisualElement();
        header.AddToClassList("formation-group-header");

        var groupIcon = new Label("👥");
        groupIcon.AddToClassList("formation-group-icon");
        header.Add(groupIcon);

        var groupName = new Label(formation.Name);
        groupName.AddToClassList("formation-group-name");
        header.Add(groupName);

        var groupCount = new Label($"{formation.Slots.Count} units");
        groupCount.AddToClassList("formation-group-count");
        header.Add(groupCount);

        group.Add(header);

        // Children container
        var children = new VisualElement();
        children.AddToClassList("formation-group-children");

        foreach (var slot in formation.Slots)
        {
            _placedAssets.Add(slot.Asset);

            var entry = CreateHierarchyEntry(slot.Asset);

            // Add leader badge
            if (slot.IsLeader)
            {
                var leaderLabel = new Label("★ Lead");
                leaderLabel.AddToClassList("unit-entry-leader");
                leaderLabel.style.fontSize = 9;
                leaderLabel.style.marginRight = 4;
                entry.Insert(2, leaderLabel);
            }

            children.Add(entry);
        }

        group.Add(children);

        // Toggle children visibility on header click
        bool expanded = true;
        header.RegisterCallback<ClickEvent>(evt =>
        {
            expanded = !expanded;
            children.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            groupIcon.text = expanded ? "👥" : "▶";
        });

        list.Add(group);
        UpdateEntityCount();
    }

    // ─────────────────────────────────────────
    //  REMOVE ENTITIES
    // ─────────────────────────────────────────

    /// <summary>Removes a single entity from the hierarchy.</summary>
    public void RemoveEntity(PlacedAsset asset)
    {
        _placedAssets.Remove(asset);

        var entry = _root.Q<VisualElement>($"entity-{asset.InstanceId}");
        entry?.parent?.Remove(entry);

        // Restore empty label if list is now empty
        CheckFactionEmpty(asset.AssignedFaction);
        UpdateEntityCount();
    }

    /// <summary>Clears all entities from the scene.</summary>
    public void ClearAll()
    {
        _placedAssets.Clear();
        _formations.Clear();

        _blueList.Clear();
        _redList.Clear();
        _neutralList.Clear();

        // Restore empty labels
        AddEmptyLabel(_blueList);
        AddEmptyLabel(_redList);
        AddEmptyLabel(_neutralList);

        UpdateEntityCount();
        OnSceneCleared?.Invoke();
    }

    // ─────────────────────────────────────────
    //  CREATE HIERARCHY ENTRY
    // ─────────────────────────────────────────

    /// <summary>
    /// Creates a single entry row for the hierarchy.
    /// </summary>
    private VisualElement CreateHierarchyEntry(PlacedAsset asset)
    {
        var entry = new VisualElement();
        entry.name = $"entity-{asset.InstanceId}";
        entry.AddToClassList("hierarchy-entry");

        // Icon
        var icon = new Label(asset.Asset.Icon);
        icon.AddToClassList("hierarchy-entry-icon");
        entry.Add(icon);

        // Name
        var name = new Label(asset.Asset.Name);
        name.AddToClassList("hierarchy-entry-name");
        entry.Add(name);

        // ID (subtle)
        var id = new Label($"#{asset.InstanceId}");
        id.AddToClassList("hierarchy-entry-id");
        entry.Add(id);

        // Remove button
        var removeBtn = new Button(() => RemoveEntity(asset));
        removeBtn.text = "✕";
        removeBtn.AddToClassList("hierarchy-entry-remove");
        entry.Add(removeBtn);

        // Click to select
        entry.RegisterCallback<ClickEvent>(evt =>
        {
            // Deselect all
            _root.Query<VisualElement>(className: "hierarchy-entry-selected").ForEach(e =>
                e.RemoveFromClassList("hierarchy-entry-selected"));

            entry.AddToClassList("hierarchy-entry-selected");
            OnEntitySelected?.Invoke(asset);
        });

        return entry;
    }

    // ─────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────

    private VisualElement GetFactionList(Faction faction)
    {
        return faction switch
        {
            Faction.BlueFOR => _blueList,
            Faction.RedFOR => _redList,
            Faction.Neutral => _neutralList,
            _ => _blueList
        };
    }

    private void ClearEmptyLabel(VisualElement list)
    {
        var emptyLabel = list.Q<Label>(className: "empty-faction-label");
        if (emptyLabel != null)
            list.Remove(emptyLabel);
    }

    private void AddEmptyLabel(VisualElement list)
    {
        var label = new Label("No units deployed");
        label.AddToClassList("empty-faction-label");
        list.Add(label);
    }

    private void CheckFactionEmpty(Faction faction)
    {
        var list = GetFactionList(faction);
        bool hasEntities = _placedAssets.Any(a => a.AssignedFaction == faction);
        if (!hasEntities)
            AddEmptyLabel(list);
    }

    private void UpdateEntityCount()
    {
        int count = _placedAssets.Count;
        _entityCount.text = $"{count} entit{(count != 1 ? "ies" : "y")}";
    }
}
