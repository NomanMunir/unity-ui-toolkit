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

## Example 3: World Space UI — NPC Health Bars

Unlike Examples 1 & 2 (screen-space overlay), this example renders UI **inside the 3D scene** — floating above NPC heads, scaling with perspective, and fading with distance.

### Screen Space vs World Space

| Aspect                   | Screen Space (Ex 1 & 2)     | World Space (Ex 3)                                |
| ------------------------ | --------------------------- | ------------------------------------------------- |
| **Rendering**            | 2D overlay on top of camera | 3D object in the scene                            |
| **Use Case**             | Menus, HUDs, inventory      | NPC labels, interaction prompts, in-world screens |
| **Position**             | Anchored to screen corners  | Attached to GameObjects                           |
| **Scales with distance** | ❌ No                       | ✅ Yes                                            |
| **PanelSettings mode**   | Screen Space (default)      | World Space                                       |

### Setup

1. **File → New Scene** → Save as `Assets/Scenes/WorldSpaceScene.unity`
2. Create **PanelSettings** asset → `Right-click → Create → UI Toolkit → Panel Settings Asset`
   - Save as `Assets/UI/WorldSpacePanelSettings`
   - Set **Render Mode** to `World Space`
   - Set **Pixels Per Unit** to `100` (adjust to taste)
3. Create empty GameObject → name `WorldSpaceManager`
4. Add `WorldSpaceSetup.cs` script
5. In the Inspector, assign:
   - **NPC UI Template**: `Assets/UI/Documents/WorldSpaceNPC.uxml`
   - **World Space Panel Settings**: `Assets/UI/WorldSpacePanelSettings`
6. Hit **Play** — 3 NPCs spawn with floating health bars

### Controls

- **WASD** = Move camera
- **Mouse** = Look around
- **Shift** = Sprint
- **E/Q** = Up/Down
- **Escape** = Toggle cursor lock

### Key Concepts Demonstrated

- **Billboard effect** — UI panels always face the camera (`WorldSpaceUIController.ApplyBillboard`)
- **Distance fading** — UI fades out when camera is far away (`ApplyDistanceFade`)
- **Inline styles for continuous values** — Health bar width set via `element.style.width`
- **USS classes for discrete states** — Status badge color switched via `AddToClassList/RemoveFromClassList`
- **Per-entity UIDocument** — Each NPC gets its own UIDocument instance

> **Unity Version Note:** World Space render mode requires Unity 6+.
> For older versions, see the "Legacy" comments in `WorldSpaceSetup.cs` for the RenderTexture workaround.

---

## Folder Structure

```
Assets/
├── UI/
│   ├── Documents/
│   │   ├── MainMenu.uxml             ← Main menu example
│   │   ├── HUD.uxml                  ← Game HUD example
│   │   ├── ScenarioCreator.uxml      ← Scenario creator (master layout)
│   │   ├── WorldSpaceNPC.uxml        ← World-space NPC health bar
│   │   └── Components/               ← Individual panel documents
│   │       ├── Toolbar.uxml
│   │       ├── AssetBrowser.uxml
│   │       ├── InspectorPanel.uxml
│   │       ├── FormationBuilder.uxml
│   │       └── SceneHierarchy.uxml
│   ├── Styles/
│   │   ├── Common.uss                ← Shared design tokens
│   │   ├── MainMenu.uss
│   │   ├── HUD.uss
│   │   ├── ScenarioCreator.uss
│   │   ├── Toolbar.uss
│   │   ├── AssetBrowser.uss
│   │   ├── InspectorPanel.uss
│   │   ├── FormationBuilder.uss
│   │   ├── SceneHierarchy.uss
│   │   └── WorldSpace.uss            ← World-space NPC UI styling
│   └── Scripts/
│       ├── MainMenuController.cs
│       ├── HUDController.cs
│       ├── ScenarioCreatorUI.cs       ← Master controller
│       ├── AssetBrowserUI.cs
│       ├── InspectorPanelUI.cs
│       ├── FormationBuilderUI.cs
│       ├── SceneHierarchyUI.cs
│       ├── WorldSpaceUIController.cs  ← Per-NPC world-space logic
│       ├── WorldSpaceSetup.cs         ← Demo scene auto-setup
│       └── Data/
│           ├── SimulationAsset.cs     ← Asset data model
│           ├── Formation.cs           ← Formation data model
│           └── AssetDatabase.cs       ← Sample data (16 assets)
└── Scenes/                            ← Create these in Unity
```
