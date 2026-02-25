/*
 * ============================================
 *  MultiDocManager.cs — Master Controller
 * ============================================
 *  The main MonoBehaviour attached to the HomePage's
 *  UIDocument GameObject. Acts as the MEDIATOR between:
 *
 *  1. NavbarController (plain C# class — handles nav links)
 *  2. HomePageController (plain C# class — handles page content)
 *  3. PopupController (MonoBehaviour on a DIFFERENT GameObject)
 *  4. ChatPanelController (MonoBehaviour on a DIFFERENT GameObject)
 *
 *  ARCHITECTURE OVERVIEW:
 *  ─────────────────────────────────────────
 *
 *  [HomePage GO]              [ChatPanel GO]         [PopupOverlay GO]
 *  UIDocument (order=0)       UIDocument (order=1)   UIDocument (order=100)
 *  MultiDocManager.cs         ChatPanelController    PopupController
 *  ├── NavbarController
 *  └── HomePageController
 *
 *  All cross-document events flow through this manager.
 *
 *  SETUP IN UNITY:
 *  ─────────────────────────────────────────
 *  1. Create "HomePage" GameObject
 *     - Add UIDocument → assign HomePage.uxml, PanelSettings
 *     - Add this script (MultiDocManager)
 *  2. Create "ChatPanel" GameObject
 *     - Add UIDocument → assign ChatPanel.uxml, same PanelSettings
 *     - Set sortingOrder = 1
 *     - Add ChatPanelController script
 *  3. Create "PopupOverlay" GameObject
 *     - Add UIDocument → assign PopupOverlay.uxml, same PanelSettings
 *     - Set sortingOrder = 100
 *     - Add PopupController script
 *  4. Drag "ChatPanel" into MultiDocManager's chatPanelController field
 *  5. Drag "PopupOverlay" into MultiDocManager's popupController field
 *  6. Play!
 */

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Master controller for the Multi-Document UI example.
/// Initializes sub-controllers and wires cross-panel & cross-document events.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MultiDocManager : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────

    [Header("Cross-Document References")]
    [Tooltip("Drag the PopupOverlay GameObject here.")]
    [SerializeField] private PopupController popupController;

    [Tooltip("Drag the ChatPanel GameObject here.")]
    [SerializeField] private ChatPanelController chatPanelController;


    // ─────────────────────────────────────────
    //  SUB-CONTROLLERS (plain C# classes)
    // ─────────────────────────────────────────
    private NavbarController _navbar;
    private HomePageController _homePage;

    private VisualElement _root;


    // ─────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        InitializeControllers();
        WireEvents();

        // Log a welcome message to the chat panel
        LogToChat("System", "Multi-Document UI initialized. Welcome!", ChatPanelController.MessageType.System);
        LogToChat("System", "3 UIDocument layers active (Main, Chat, Popup).", ChatPanelController.MessageType.System);

        Debug.Log("[MultiDocManager] All controllers initialized and events wired.");
    }


    // ─────────────────────────────────────────
    //  INITIALIZATION
    // ─────────────────────────────────────────

    private void InitializeControllers()
    {
        // ── Navbar Controller ──
        var navbarRoot = _root.Q<VisualElement>("navbar");
        _navbar = new NavbarController();
        _navbar.Initialize(navbarRoot);

        // ── HomePage Controller ──
        var pageBody = _root.Q<VisualElement>("page-body");
        _homePage = new HomePageController();
        _homePage.Initialize(pageBody);
    }


    // ─────────────────────────────────────────
    //  EVENT WIRING — The Mediator Pattern
    // ─────────────────────────────────────────

    private void WireEvents()
    {
        // ── Navbar → Chat Panel (cross-document!) ──
        _navbar.OnToggleChatRequested += () =>
        {
            if (chatPanelController != null)
            {
                chatPanelController.Toggle();
            }
        };

        // ── Navbar → Popup (cross-document!) ──
        _navbar.OnShowPopupRequested += () =>
        {
            if (popupController != null)
            {
                popupController.ShowWithMessage(
                    "Multi-Document Popup",
                    "This popup lives on a separate UIDocument with sortingOrder = 100. " +
                    "It renders above the main page and has its own controller.",
                    "🔔"
                );
                LogToChat("Event", "Popup opened from navbar.", ChatPanelController.MessageType.Event);
            }
            else
            {
                Debug.LogWarning("[MultiDocManager] PopupController reference not set!");
            }
        };

        // ── Navbar → Page content + Chat log ──
        _navbar.OnNavLinkClicked += (linkName) =>
        {
            Debug.Log($"[MultiDocManager] Navigation: {linkName}");
            LogToChat("Navigation", $"Switched to: {linkName}", ChatPanelController.MessageType.Event);
        };

        // ── HomePage → Popup (show member details) ──
        _homePage.OnCardDetailsClicked += (memberName) =>
        {
            if (popupController != null)
            {
                popupController.ShowWithMessage(
                    $"About {memberName}",
                    $"Viewing details for team member: {memberName}. " +
                    "In a real app, this would show full profile info.",
                    "👤"
                );
            }
            LogToChat("Event", $"Viewed details for {memberName}.", ChatPanelController.MessageType.Event);
        };

        // ── Popup → Main page (cross-document events!) ──
        if (popupController != null)
        {
            popupController.OnConfirm += () =>
            {
                Debug.Log("[MultiDocManager] Popup confirmed.");
                LogToChat("Action", "Popup confirmed ✔", ChatPanelController.MessageType.Success);
            };

            popupController.OnCancel += () =>
            {
                Debug.Log("[MultiDocManager] Popup cancelled.");
                LogToChat("Action", "Popup dismissed ✖", ChatPanelController.MessageType.Error);
            };
        }

        // ── Chat panel → Main page ──
        if (chatPanelController != null)
        {
            chatPanelController.OnMessageSent += (message) =>
            {
                Debug.Log($"[MultiDocManager] Chat message received: {message}");
                // In a real app this could trigger game commands, etc.
            };
        }
    }


    // ─────────────────────────────────────────
    //  HELPER — Log to Chat Panel
    // ─────────────────────────────────────────

    /// <summary>
    /// Safely logs a message to the chat panel (if available).
    /// This is the cross-document data flow in action:
    ///   Action on Page → Manager catches event → Logs to Chat (different UIDocument)
    /// </summary>
    private void LogToChat(string sender, string text, ChatPanelController.MessageType type)
    {
        if (chatPanelController != null)
        {
            chatPanelController.AddMessage(sender, text, type);
        }
    }
}
