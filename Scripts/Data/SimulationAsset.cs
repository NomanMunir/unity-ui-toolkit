/*
 * ============================================
 *  SimulationAsset.cs — Data Model for Assets
 * ============================================
 *  Defines what a simulation asset looks like.
 *  Used to populate the Asset Browser and Inspector.
 */

using System;
using System.Collections.Generic;

/// <summary>
/// Categories for organizing assets in the browser.
/// </summary>
public enum AssetCategory
{
    Aircraft,
    GroundVehicles,
    Naval,
    Infantry,
    Structures,
    Sensors
}

/// <summary>
/// Faction/team assignment for color coding.
/// </summary>
public enum Faction
{
    BlueFOR,    // Friendly forces (blue)
    RedFOR,     // Enemy forces (red)
    Neutral     // Neutral entities (green)
}

/// <summary>
/// A single configurable property on an asset.
/// Used to dynamically generate Inspector fields.
/// </summary>
[Serializable]
public class AssetProperty
{
    public string Name;
    public string Key;
    public PropertyType Type;
    public float FloatValue;
    public float MinValue;
    public float MaxValue;
    public string StringValue;
    public int IntValue;
    public bool BoolValue;
    public List<string> DropdownOptions;
    public int DropdownIndex;
    public string Unit; // e.g., "km/h", "ft", "°"

    public enum PropertyType
    {
        Float,
        Int,
        Bool,
        String,
        Dropdown
    }

    /// <summary>Creates a float slider property.</summary>
    public static AssetProperty FloatSlider(string name, string key, float value, float min, float max, string unit = "")
    {
        return new AssetProperty
        {
            Name = name, Key = key, Type = PropertyType.Float,
            FloatValue = value, MinValue = min, MaxValue = max, Unit = unit
        };
    }

    /// <summary>Creates a dropdown property.</summary>
    public static AssetProperty Dropdown(string name, string key, List<string> options, int defaultIndex = 0)
    {
        return new AssetProperty
        {
            Name = name, Key = key, Type = PropertyType.Dropdown,
            DropdownOptions = options, DropdownIndex = defaultIndex
        };
    }

    /// <summary>Creates a boolean toggle property.</summary>
    public static AssetProperty Toggle(string name, string key, bool value)
    {
        return new AssetProperty
        {
            Name = name, Key = key, Type = PropertyType.Bool, BoolValue = value
        };
    }

    /// <summary>Creates an integer field property.</summary>
    public static AssetProperty IntField(string name, string key, int value, int min, int max, string unit = "")
    {
        return new AssetProperty
        {
            Name = name, Key = key, Type = PropertyType.Int,
            IntValue = value, MinValue = min, MaxValue = max, Unit = unit
        };
    }
}

/// <summary>
/// Represents a simulation asset (aircraft, vehicle, unit, etc.)
/// that can be placed in the scenario.
/// </summary>
[Serializable]
public class SimulationAsset
{
    public string Id;
    public string Name;
    public string Description;
    public string Icon;           // Emoji/text icon for now, replace with Texture2D later
    public AssetCategory Category;
    public List<AssetProperty> Properties;

    public SimulationAsset(string id, string name, string description, string icon,
                           AssetCategory category, List<AssetProperty> properties)
    {
        Id = id;
        Name = name;
        Description = description;
        Icon = icon;
        Category = category;
        Properties = properties ?? new List<AssetProperty>();
    }
}

/// <summary>
/// An instance of an asset placed in the scene, with its
/// configured properties and faction assignment.
/// </summary>
[Serializable]
public class PlacedAsset
{
    public string InstanceId;
    public SimulationAsset Asset;
    public Faction AssignedFaction;
    public string FormationId;         // null if standalone
    public List<AssetProperty> ConfiguredProperties;

    public PlacedAsset(SimulationAsset asset, Faction faction)
    {
        InstanceId = Guid.NewGuid().ToString().Substring(0, 8);
        Asset = asset;
        AssignedFaction = faction;
        FormationId = null;

        // Deep copy properties so each instance has its own values
        ConfiguredProperties = new List<AssetProperty>();
        foreach (var prop in asset.Properties)
        {
            ConfiguredProperties.Add(new AssetProperty
            {
                Name = prop.Name,
                Key = prop.Key,
                Type = prop.Type,
                FloatValue = prop.FloatValue,
                MinValue = prop.MinValue,
                MaxValue = prop.MaxValue,
                StringValue = prop.StringValue,
                IntValue = prop.IntValue,
                BoolValue = prop.BoolValue,
                DropdownOptions = prop.DropdownOptions != null ? new List<string>(prop.DropdownOptions) : null,
                DropdownIndex = prop.DropdownIndex,
                Unit = prop.Unit
            });
        }
    }
}
