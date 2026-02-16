/*
 * ============================================
 *  ScenarioCreatorUI.cs — Master Controller
 * ============================================
 *  The main MonoBehaviour that initializes all panels
 *  and wires them together. Attach this to a GameObject
 *  with a UIDocument component.
 *
 *  ARCHITECTURE:
 *  This follows a mediator pattern — the master controller
 *  listens to events from each panel and coordinates
 *  cross-panel communication.
 *
 *  SETUP IN UNITY:
 *  1. Create an empty GameObject named "ScenarioCreator"
 *  2. Add UIDocument component
 *  3. Assign ScenarioCreator.uxml as the Source Asset
 *  4. Assign your PanelSettings
 *  5. Add this script
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Master controller for the Scenario Creator workspace.
/// Initializes all panel controllers and wires events between them.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ScenarioCreatorUI : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  PANEL CONTROLLERS (not MonoBehaviours — plain C# classes)
    // ─────────────────────────────────────────
    private AssetBrowserUI _assetBrowser;
    private InspectorPanelUI _inspector;
    private FormationBuilderUI _formationBuilder;
    private SceneHierarchyUI _sceneHierarchy;

    private VisualElement _root;


    // ─────────────────────────────────────────
    //  INITIALIZATION
    // ─────────────────────────────────────────

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        InitializePanels();
        WireEvents();
        RegisterToolbarEvents();

        Debug.Log("Scenario Creator UI initialized successfully.");
    }

    /// <summary>
    /// Creates and initializes each panel controller.
    /// Each controller receives its own root element to work with.
    /// </summary>
    private void InitializePanels()
    {
        // Asset Browser (left sidebar)
        _assetBrowser = new AssetBrowserUI();
        _assetBrowser.Initialize(_root.Q<VisualElement>("asset-browser"));

        // Inspector (right sidebar)
        _inspector = new InspectorPanelUI();
        _inspector.Initialize(_root.Q<VisualElement>("inspector-panel"));

        // Formation Builder (bottom-left)
        _formationBuilder = new FormationBuilderUI();
        _formationBuilder.Initialize(_root.Q<VisualElement>("formation-builder"));

        // Scene Hierarchy (bottom-right)
        _sceneHierarchy = new SceneHierarchyUI();
        _sceneHierarchy.Initialize(_root.Q<VisualElement>("scene-hierarchy"));
    }

    // ─────────────────────────────────────────
    //  EVENT WIRING — Cross-Panel Communication
    // ─────────────────────────────────────────

    /// <summary>
    /// Connects events between panels.
    /// This is the mediator — panels don't know about each other,
    /// they only fire events that the master controller routes.
    ///
    ///    AssetBrowser ──(selected)──> Inspector
    ///    Inspector ──(add to scene)──> SceneHierarchy
    ///    Inspector ──(add to formation)──> FormationBuilder
    ///    FormationBuilder ──(drop)──> SceneHierarchy
    /// </summary>
    private void WireEvents()
    {
        // Browser → Inspector: When an asset tile is clicked, show in inspector
        _assetBrowser.OnAssetSelected += (asset) =>
        {
            _inspector.ShowAsset(asset);
            Debug.Log($"Selected: {asset.Name} ({asset.Category})");
        };

        // Inspector → Scene: When "Add to Scene" is clicked
        _inspector.OnAddToScene += (asset, faction, quantity) =>
        {
            for (int i = 0; i < quantity; i++)
            {
                var placed = new PlacedAsset(asset, faction);
                _sceneHierarchy.AddEntity(placed);
                Debug.Log($"Added to scene: {asset.Name} [{faction}] #{placed.InstanceId}");
            }
        };

        // Inspector → Formation: When "Add to Formation" is clicked
        _inspector.OnAddToFormation += (asset, faction, quantity) =>
        {
            for (int i = 0; i < quantity; i++)
            {
                var placed = new PlacedAsset(asset, faction);
                _formationBuilder.AddUnit(placed);
                Debug.Log($"Added to formation: {asset.Name} [{faction}]");
            }
        };

        // Formation → Scene: When a formation is dropped
        _formationBuilder.OnFormationDropped += (formation) =>
        {
            _sceneHierarchy.AddFormation(formation);
            Debug.Log($"Formation '{formation.Name}' added to scene with {formation.Slots.Count} units.");
        };

        // Hierarchy → Inspector: When a hierarchy entry is clicked
        _sceneHierarchy.OnEntitySelected += (placed) =>
        {
            _inspector.ShowAsset(placed.Asset);
            Debug.Log($"Hierarchy selected: {placed.Asset.Name} #{placed.InstanceId}");
        };

        // Hierarchy → Clear all
        _sceneHierarchy.OnSceneCleared += () =>
        {
            Debug.Log("Scene cleared — all entities removed.");
        };
    }

    // ─────────────────────────────────────────
    //  TOOLBAR EVENTS
    // ─────────────────────────────────────────

    private void RegisterToolbarEvents()
    {
        _root.Q<Button>("btn-new")?.RegisterCallback<ClickEvent>(evt =>
        {
            _sceneHierarchy.ClearAll();
            _formationBuilder.CreateNewFormation();
            _inspector.ClearSelection();
            Debug.Log("New scenario created.");
        });

        _root.Q<Button>("btn-save")?.RegisterCallback<ClickEvent>(evt =>
        {
            string name = _root.Q<TextField>("scenario-name")?.value ?? "Untitled";
            Debug.Log($"Save scenario: '{name}' (not implemented — placeholder)");
        });

        _root.Q<Button>("btn-load")?.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("Load scenario (not implemented — placeholder)");
        });

        _root.Q<Button>("btn-play-sim")?.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("▶ Play simulation (not implemented — placeholder)");
        });

        _root.Q<Button>("btn-pause-sim")?.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("⏸ Pause simulation (not implemented — placeholder)");
        });

        _root.Q<Button>("btn-stop-sim")?.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("⏹ Stop simulation (not implemented — placeholder)");
        });

        _root.Q<Button>("btn-undo")?.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("↩ Undo (not implemented — placeholder)");
        });

        _root.Q<Button>("btn-redo")?.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("↪ Redo (not implemented — placeholder)");
        });

        _root.Q<Button>("btn-settings")?.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("⚙ Settings (not implemented — placeholder)");
        });
    }
}
