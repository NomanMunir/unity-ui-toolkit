/*
 * ============================================
 *  Formation.cs — Formation Data Model
 * ============================================
 *  Defines how units are grouped and positioned
 *  relative to each other in a formation.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Preset formation patterns.
/// </summary>
public enum FormationType
{
    VFormation,
    LineAbreast,
    Column,
    Diamond,
    Echelon,
    Custom
}

/// <summary>
/// A unit within a formation with its relative position.
/// Positions are normalized (-1 to 1) for display in the mini canvas.
/// </summary>
[Serializable]
public class FormationSlot
{
    public PlacedAsset Asset;
    public Vector2 RelativePosition; // Normalized position in formation canvas
    public bool IsLeader;

    public FormationSlot(PlacedAsset asset, Vector2 position, bool isLeader = false)
    {
        Asset = asset;
        RelativePosition = position;
        IsLeader = isLeader;
    }
}

/// <summary>
/// A formation is a group of units with defined relative positions.
/// Can be dropped into the scene as a single entity.
/// </summary>
[Serializable]
public class Formation
{
    public string Id;
    public string Name;
    public FormationType Type;
    public Faction AssignedFaction;
    public List<FormationSlot> Slots;

    public Formation(string name, FormationType type, Faction faction)
    {
        Id = Guid.NewGuid().ToString().Substring(0, 8);
        Name = name;
        Type = type;
        AssignedFaction = faction;
        Slots = new List<FormationSlot>();
    }

    /// <summary>
    /// Adds a unit to the formation and auto-arranges based on formation type.
    /// </summary>
    public void AddUnit(PlacedAsset asset)
    {
        bool isLeader = Slots.Count == 0;
        var slot = new FormationSlot(asset, Vector2.zero, isLeader);
        asset.FormationId = Id;
        Slots.Add(slot);
        ArrangeSlots();
    }

    /// <summary>
    /// Removes a unit from the formation.
    /// </summary>
    public void RemoveUnit(PlacedAsset asset)
    {
        Slots.RemoveAll(s => s.Asset.InstanceId == asset.InstanceId);
        asset.FormationId = null;

        // Reassign leader if needed
        if (Slots.Count > 0 && !Slots.Exists(s => s.IsLeader))
            Slots[0].IsLeader = true;

        ArrangeSlots();
    }

    /// <summary>
    /// Rearranges unit positions based on the current formation type.
    /// </summary>
    public void ArrangeSlots()
    {
        switch (Type)
        {
            case FormationType.VFormation:   ArrangeV(); break;
            case FormationType.LineAbreast:  ArrangeLine(); break;
            case FormationType.Column:       ArrangeColumn(); break;
            case FormationType.Diamond:      ArrangeDiamond(); break;
            case FormationType.Echelon:      ArrangeEchelon(); break;
            case FormationType.Custom:       break; // Don't rearrange
        }
    }

    private void ArrangeV()
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (i == 0)
            {
                Slots[i].RelativePosition = new Vector2(0f, 0.3f); // Lead in front
            }
            else
            {
                int side = (i % 2 == 1) ? -1 : 1; // Alternate left/right
                int depth = (i + 1) / 2;
                float x = side * depth * 0.2f;
                float y = 0.3f - depth * 0.2f;
                Slots[i].RelativePosition = new Vector2(x, y);
            }
        }
    }

    private void ArrangeLine()
    {
        float spacing = 0.8f / Mathf.Max(1, Slots.Count - 1);
        for (int i = 0; i < Slots.Count; i++)
        {
            float x = -0.4f + i * spacing;
            Slots[i].RelativePosition = new Vector2(x, 0f);
        }
    }

    private void ArrangeColumn()
    {
        float spacing = 0.7f / Mathf.Max(1, Slots.Count - 1);
        for (int i = 0; i < Slots.Count; i++)
        {
            float y = 0.35f - i * spacing;
            Slots[i].RelativePosition = new Vector2(0f, y);
        }
    }

    private void ArrangeDiamond()
    {
        Vector2[] positions = {
            new Vector2(0f,  0.3f),   // Top
            new Vector2(-0.3f, 0f),   // Left
            new Vector2(0.3f,  0f),   // Right
            new Vector2(0f, -0.3f),   // Bottom
            new Vector2(-0.2f, 0.15f),
            new Vector2(0.2f, 0.15f),
            new Vector2(-0.2f, -0.15f),
            new Vector2(0.2f, -0.15f),
        };

        for (int i = 0; i < Slots.Count; i++)
        {
            Slots[i].RelativePosition = i < positions.Length
                ? positions[i]
                : new Vector2(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(-0.3f, 0.3f));
        }
    }

    private void ArrangeEchelon()
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            float x = -0.3f + i * 0.15f;
            float y = 0.3f - i * 0.15f;
            Slots[i].RelativePosition = new Vector2(x, y);
        }
    }
}
