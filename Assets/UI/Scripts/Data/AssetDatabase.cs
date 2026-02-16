/*
 * ============================================
 *  AssetDatabase.cs — Sample Asset Data
 * ============================================
 *  Provides hardcoded sample assets for the demo.
 *  In a real project, this would load from ScriptableObjects,
 *  JSON files, or an actual database.
 */

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides sample simulation assets for populating the Asset Browser.
/// Replace this with real data loading in production.
/// </summary>
public static class SimAssetDatabase
{
    private static List<SimulationAsset> _assets;

    /// <summary>
    /// Gets all available assets.
    /// </summary>
    public static List<SimulationAsset> GetAllAssets()
    {
        if (_assets == null)
            _assets = CreateSampleAssets();
        return _assets;
    }

    /// <summary>
    /// Gets assets filtered by category.
    /// </summary>
    public static List<SimulationAsset> GetByCategory(AssetCategory category)
    {
        return GetAllAssets().Where(a => a.Category == category).ToList();
    }

    /// <summary>
    /// Searches assets by name (case-insensitive).
    /// </summary>
    public static List<SimulationAsset> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAllAssets();

        string lowerQuery = query.ToLower();
        return GetAllAssets()
            .Where(a => a.Name.ToLower().Contains(lowerQuery) ||
                        a.Description.ToLower().Contains(lowerQuery))
            .ToList();
    }

    /// <summary>
    /// Gets a single asset by ID.
    /// </summary>
    public static SimulationAsset GetById(string id)
    {
        return GetAllAssets().FirstOrDefault(a => a.Id == id);
    }

    // ─────────────────────────────────────────
    //  SAMPLE DATA
    // ─────────────────────────────────────────

    private static List<SimulationAsset> CreateSampleAssets()
    {
        return new List<SimulationAsset>
        {
            // ========== AIRCRAFT ==========
            new SimulationAsset("f16", "F-16 Falcon", "Multirole fighter aircraft", "✈",
                AssetCategory.Aircraft, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 900, 200, 1500, "km/h"),
                    AssetProperty.FloatSlider("Altitude", "altitude", 8000, 100, 15000, "m"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Loadout", "loadout",
                        new List<string> { "Air-to-Air", "Air-to-Ground", "Recon", "Clean" }, 0),
                    AssetProperty.Toggle("Afterburner", "afterburner", false),
                    AssetProperty.IntField("Fuel %", "fuel", 80, 0, 100, "%"),
                }),

            new SimulationAsset("f35", "F-35 Lightning", "5th gen stealth fighter", "✈",
                AssetCategory.Aircraft, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 850, 200, 1900, "km/h"),
                    AssetProperty.FloatSlider("Altitude", "altitude", 10000, 100, 15000, "m"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Mode", "mode",
                        new List<string> { "Air Superiority", "Strike", "SEAD", "ISR" }, 0),
                    AssetProperty.Toggle("Stealth Mode", "stealth", true),
                    AssetProperty.IntField("Fuel %", "fuel", 90, 0, 100, "%"),
                }),

            new SimulationAsset("su30", "Su-30 Flanker", "Twin-engine air superiority fighter", "✈",
                AssetCategory.Aircraft, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 1000, 200, 2100, "km/h"),
                    AssetProperty.FloatSlider("Altitude", "altitude", 9000, 100, 17000, "m"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Loadout", "loadout",
                        new List<string> { "Air-to-Air", "Multi-Role", "Strike" }, 0),
                    AssetProperty.IntField("Fuel %", "fuel", 85, 0, 100, "%"),
                }),

            new SimulationAsset("mq9", "MQ-9 Reaper", "Unmanned aerial vehicle (UAV)", "🛩",
                AssetCategory.Aircraft, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 370, 100, 480, "km/h"),
                    AssetProperty.FloatSlider("Altitude", "altitude", 7500, 500, 15000, "m"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Payload", "payload",
                        new List<string> { "ISR Only", "Hellfire", "GBU-12", "Mixed" }, 0),
                    AssetProperty.IntField("Endurance", "endurance", 20, 1, 27, "hrs"),
                }),

            new SimulationAsset("ah64", "AH-64 Apache", "Attack helicopter", "🚁",
                AssetCategory.Aircraft, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 250, 0, 365, "km/h"),
                    AssetProperty.FloatSlider("Altitude", "altitude", 500, 10, 6400, "m"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Weapons", "weapons",
                        new List<string> { "Hellfire + Chain Gun", "Rockets + Chain Gun", "Full Loadout" }, 2),
                }),

            // ========== GROUND VEHICLES ==========
            new SimulationAsset("m1a2", "M1A2 Abrams", "Main battle tank", "🛡",
                AssetCategory.GroundVehicles, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 40, 0, 67, "km/h"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Ammo Type", "ammo",
                        new List<string> { "APFSDS", "HEAT", "Canister", "MPAT" }, 0),
                    AssetProperty.Toggle("Active Protection", "aps", true),
                    AssetProperty.IntField("Crew", "crew", 4, 1, 4, ""),
                }),

            new SimulationAsset("bradley", "M2 Bradley", "Infantry fighting vehicle", "🛡",
                AssetCategory.GroundVehicles, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 50, 0, 66, "km/h"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Variant", "variant",
                        new List<string> { "M2 (Infantry)", "M3 (Cavalry)" }, 0),
                    AssetProperty.IntField("Dismounts", "dismounts", 6, 0, 7, ""),
                }),

            new SimulationAsset("humvee", "HMMWV", "High mobility multipurpose wheeled vehicle", "🚙",
                AssetCategory.GroundVehicles, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 80, 0, 113, "km/h"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Variant", "variant",
                        new List<string> { "TOW Missile", "M2 .50 Cal", "Mk19 Grenade", "Transport" }, 3),
                }),

            // ========== NAVAL ==========
            new SimulationAsset("ddg", "DDG-51 Destroyer", "Arleigh Burke-class guided missile destroyer", "🚢",
                AssetCategory.Naval, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 20, 0, 56, "km/h"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Radar Mode", "radar",
                        new List<string> { "Search", "Track", "Engagement", "Silent" }, 0),
                    AssetProperty.Toggle("AEGIS Active", "aegis", true),
                    AssetProperty.IntField("VLS Cells", "vls", 96, 0, 96, ""),
                }),

            new SimulationAsset("frigate", "FFG Frigate", "Guided missile frigate", "🚢",
                AssetCategory.Naval, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 22, 0, 52, "km/h"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Mission", "mission",
                        new List<string> { "ASW", "Surface Warfare", "Escort", "Patrol" }, 0),
                }),

            // ========== INFANTRY ==========
            new SimulationAsset("rifle_sq", "Rifle Squad", "9-man infantry rifle squad", "🎖",
                AssetCategory.Infantry, new List<AssetProperty>
                {
                    AssetProperty.IntField("Squad Size", "size", 9, 4, 13, ""),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Posture", "posture",
                        new List<string> { "Patrol", "Assault", "Defend", "Recon" }, 0),
                    AssetProperty.Dropdown("Equipment", "equipment",
                        new List<string> { "Standard", "AT (Javelin)", "AA (Stinger)", "Mortar" }, 0),
                }),

            new SimulationAsset("sniper", "Sniper Team", "2-man sniper/spotter team", "🎯",
                AssetCategory.Infantry, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Dropdown("Rifle", "rifle",
                        new List<string> { "M110 (7.62mm)", "M107 (.50 Cal)", "MSR (.338)" }, 0),
                    AssetProperty.Toggle("Ghillie Suit", "ghillie", true),
                }),

            // ========== STRUCTURES ==========
            new SimulationAsset("fob", "FOB", "Forward operating base", "🏗",
                AssetCategory.Structures, new List<AssetProperty>
                {
                    AssetProperty.Dropdown("Size", "size",
                        new List<string> { "Small (Platoon)", "Medium (Company)", "Large (Battalion)" }, 1),
                    AssetProperty.Toggle("Helipad", "helipad", true),
                    AssetProperty.Toggle("Radar Station", "radar", false),
                }),

            new SimulationAsset("sam_site", "SAM Site", "Surface-to-air missile battery", "📡",
                AssetCategory.Structures, new List<AssetProperty>
                {
                    AssetProperty.Dropdown("System", "system",
                        new List<string> { "Patriot (Long Range)", "NASAMS (Medium)", "Avenger (Short)" }, 0),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.Toggle("Radar Active", "radar_active", false),
                    AssetProperty.IntField("Missiles", "missiles", 8, 0, 16, ""),
                }),

            // ========== SENSORS ==========
            new SimulationAsset("awacs", "E-3 AWACS", "Airborne early warning aircraft", "📡",
                AssetCategory.Sensors, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Speed", "speed", 750, 300, 860, "km/h"),
                    AssetProperty.FloatSlider("Altitude", "altitude", 9000, 5000, 12000, "m"),
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.FloatSlider("Scan Radius", "scan_radius", 400, 100, 650, "km"),
                }),

            new SimulationAsset("ground_radar", "Ground Radar", "Ground-based surveillance radar", "📡",
                AssetCategory.Sensors, new List<AssetProperty>
                {
                    AssetProperty.FloatSlider("Heading", "heading", 0, 0, 360, "°"),
                    AssetProperty.FloatSlider("Range", "range", 200, 50, 500, "km"),
                    AssetProperty.Dropdown("Mode", "mode",
                        new List<string> { "Air Search", "Ground Search", "Track", "Off" }, 0),
                    AssetProperty.Toggle("IFF Active", "iff", true),
                }),
        };
    }
}
