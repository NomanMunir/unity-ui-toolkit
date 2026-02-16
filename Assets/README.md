# Unity UI Toolkit — Learning Examples

## 📋 Setup Instructions

### 1. Copy Files

Copy the entire `Assets/` folder contents into your Unity project's `Assets/` folder.

### 2. Create Scenes

In Unity:

1. **File → New Scene** → Save as `Assets/Scenes/MainMenuScene.unity`
2. **File → New Scene** → Save as `Assets/Scenes/GameScene.unity`

### 3. Setup Main Menu Scene (`MainMenuScene`)

1. Open `MainMenuScene`
2. Create an empty GameObject → name it `UIManager`
3. Add the `MainMenuController.cs` script to it
4. In the Inspector, assign:
   - **Menu Document**: drag `Assets/UI/Documents/MainMenu.uxml`
   - **Menu Stylesheet**: drag `Assets/UI/Styles/MainMenu.uss`

### 4. Setup Game Scene (`GameScene`)

1. Open `GameScene`
2. Create an empty GameObject → name it `HUDManager`
3. Add the `HUDController.cs` script to it
4. In the Inspector, assign:
   - **HUD Document**: drag `Assets/UI/Documents/HUD.uxml`
   - **HUD Stylesheet**: drag `Assets/UI/Styles/HUD.uss`

### 5. Add Scenes to Build Settings

1. **File → Build Settings**
2. Add both scenes (MainMenuScene at index 0, GameScene at index 1)

### 6. Required Components

Each scene needs a **UIDocument** component on a GameObject:

- The controller scripts handle this automatically via `[RequireComponent(typeof(UIDocument))]`

---

## 📁 Folder Structure

```
Assets/
├── UI/
│   ├── Documents/          # UXML files (structure — like HTML)
│   │   ├── MainMenu.uxml
│   │   ├── SettingsPanel.uxml
│   │   └── HUD.uxml
│   ├── Styles/             # USS files (styling — like CSS)
│   │   ├── Common.uss
│   │   ├── MainMenu.uss
│   │   └── HUD.uss
│   └── Scripts/            # C# scripts (logic — like JS)
│       ├── MainMenuController.cs
│       ├── SettingsController.cs
│       └── HUDController.cs
├── Scenes/
│   ├── MainMenuScene.unity  # (create in Unity)
│   └── GameScene.unity      # (create in Unity)
└── README.md
```

## 🧠 Learning Notes

### UXML = HTML equivalent

- Defines **what** elements exist and their hierarchy
- Uses XML syntax with Unity-specific tags
- Elements have `name` attributes for C# queries (like `id` in HTML)

### USS = CSS equivalent

- Defines **how** elements look
- Supports selectors: `#name`, `.class`, `Type`
- Uses Unity-specific properties (similar but not identical to CSS)
- Supports pseudo-classes: `:hover`, `:active`, `:focus`, `:checked`

### C# = JavaScript equivalent

- Queries elements using `rootVisualElement.Q<Type>("name")`
- Registers event callbacks (like `addEventListener`)
- Manipulates the visual tree at runtime
