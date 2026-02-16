# Unity UI Toolkit — Learning Examples

## Example 1: Main Menu + HUD

### Setup Main Menu Scene

1. **File → New Scene** → Save as `Assets/Scenes/MainMenuScene.unity`
2. Create empty GameObject → name `UIManager`
3. Add `MainMenuController.cs` script
4. On the **UIDocument** component:
   - **Panel Settings**: Create via `Right-click → Create → UI Toolkit → Panel Settings Asset` (save as `Assets/UI/DefaultPanelSettings`) or reuse existing
   - **Source Asset**: `Assets/UI/Documents/MainMenu.uxml`
5. Recommended Panel Settings: Scale Mode = `Scale With Screen Size`, Reference Resolution = `1920×1080`

### Setup Game Scene

1. **File → New Scene** → Save as `Assets/Scenes/GameScene.unity`
2. Create empty GameObject → name `HUDManager`
3. Add `HUDController.cs` script
4. On the **UIDocument** component:
   - **Panel Settings**: Same `DefaultPanelSettings`
   - **Source Asset**: `Assets/UI/Documents/HUD.uxml`
5. Add both scenes to **File → Build Profiles** (MainMenuScene=0, GameScene=1)

---

## Example 2: Scenario Creator (4-Panel Workspace)

A simulation scenario creation tool with asset browser, inspector, formation builder, and scene hierarchy.

### Setup

1. **File → New Scene** → Save as `Assets/Scenes/ScenarioCreatorScene.unity`
2. Create empty GameObject → name `ScenarioCreator`
3. Add `ScenarioCreatorUI.cs` script
4. On the **UIDocument** component:
   - **Panel Settings**: Same `DefaultPanelSettings`
   - **Source Asset**: `Assets/UI/Documents/ScenarioCreator.uxml`
5. Hit **Play** — everything is wired automatically

### How It Works

- **Left**: Asset Browser — click tiles to select assets
- **Right**: Inspector — configure properties, choose faction, add to scene or formation
- **Bottom-Left**: Formation Builder — group units, choose preset arrangements (V, Line, Diamond...)
- **Bottom-Right**: Scene Hierarchy — tracks all placed entities by faction

### Workflow

1. Click an asset tile in the browser (e.g., F-16)
2. Configure properties in the inspector (speed, altitude, loadout)
3. Choose faction (Blue/Red/Neutral)
4. Either "Add to Scene" directly, or "Add to Formation"
5. In formation builder: add multiple units, pick arrangement preset
6. Click "Drop to Scene" to place the whole formation

---

## Folder Structure

```
Assets/
├── UI/
│   ├── Documents/
│   │   ├── MainMenu.uxml          ← Main menu example
│   │   ├── HUD.uxml               ← Game HUD example
│   │   ├── ScenarioCreator.uxml   ← Scenario creator (master layout)
│   │   └── Components/            ← Individual panel documents
│   │       ├── Toolbar.uxml
│   │       ├── AssetBrowser.uxml
│   │       ├── InspectorPanel.uxml
│   │       ├── FormationBuilder.uxml
│   │       └── SceneHierarchy.uxml
│   ├── Styles/
│   │   ├── Common.uss             ← Shared design tokens
│   │   ├── MainMenu.uss
│   │   ├── HUD.uss
│   │   ├── ScenarioCreator.uss
│   │   ├── Toolbar.uss
│   │   ├── AssetBrowser.uss
│   │   ├── InspectorPanel.uss
│   │   ├── FormationBuilder.uss
│   │   └── SceneHierarchy.uss
│   └── Scripts/
│       ├── MainMenuController.cs
│       ├── HUDController.cs
│       ├── ScenarioCreatorUI.cs   ← Master controller
│       ├── AssetBrowserUI.cs
│       ├── InspectorPanelUI.cs
│       ├── FormationBuilderUI.cs
│       ├── SceneHierarchyUI.cs
│       └── Data/
│           ├── SimulationAsset.cs  ← Asset data model
│           ├── Formation.cs        ← Formation data model
│           └── AssetDatabase.cs    ← Sample data (16 assets)
└── Scenes/                         ← Create these in Unity
```
