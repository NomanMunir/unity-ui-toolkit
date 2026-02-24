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
 *
 *  ARCHITECTURE OVERVIEW:
 *  ─────────────────────────────────────────
 *
 *  [HomePage GameObject]           [PopupOverlay GameObject]
 *  ├── UIDocument (sortingOrder=0) ├── UIDocument (sortingOrder=100)
 *  ├── MultiDocManager.cs          └── PopupController.cs
 *  │   ├── NavbarController ←─ event ──┐
 *  │   ├── HomePageController          │
 *  │   └── _popupController ──────→ [SerializeField ref]
 *  │                                   │
 *  │   OnShowPopupRequested ───────────┘
 *  │   OnConfirm / OnCancel ←──────── PopupController events
 *
 *  WHY A MEDIATOR?
 *  ─────────────────────────────────────────
 *  Sub-controllers (Navbar, HomePage) don't know about each other
 *  or about the popup. They only fire events. The master controller
 *  routes those events to the right destination.
 *
 *  This keeps each controller focused on ONE responsibility:
 *  - NavbarController: nav link state + "show popup" button
 *  - HomePageController: card data + card click events
 *  - PopupController: show/hide popup + confirm/cancel
 *  - MultiDocManager: WIRING between all of them
 *
 *  SETUP IN UNITY:
 *  ─────────────────────────────────────────
 *  1. Create "HomePage" GameObject
 *     - Add UIDocument → assign HomePage.uxml, PanelSettings
 *     - Add this script (MultiDocManager)
 *  2. Create "PopupOverlay" GameObject
 *     - Add UIDocument → assign PopupOverlay.uxml, same PanelSettings
 *     - Set sortingOrder = 100
 *     - Add PopupController script
 *     - DISABLE the GameObject (uncheck in Inspector)
 *  3. Drag "PopupOverlay" into MultiDocManager's "popupController" field
 *  4. Play!
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
    [Tooltip("Drag the PopupOverlay GameObject here. It has its own UIDocument + PopupController.")]
    [SerializeField] private PopupController popupController;


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
        // Get THIS document's visual tree root
        var uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        InitializeControllers();
        WireEvents();

        Debug.Log("[MultiDocManager] All controllers initialized and events wired.");
    }


    // ─────────────────────────────────────────
    //  INITIALIZATION
    // ─────────────────────────────────────────

    /// <summary>
    /// Creates sub-controllers and passes them their root elements.
    ///
    /// IMPORTANT: Q<T>() searches ALL descendants, including inside
    /// template instances (TemplateContainers). So even though
    /// "navbar" is defined in Navbar.uxml, we can find it here
    /// because it's been composed into HomePage.uxml via ui:Instance.
    /// </summary>
    private void InitializeControllers()
    {
        // ── Navbar Controller ──
        // The Navbar component was placed via <ui:Instance template="Navbar" name="navbar-instance" />
        // We can query the navbar root directly — Q<T>() crosses template boundaries.
        var navbarRoot = _root.Q<VisualElement>("navbar");
        _navbar = new NavbarController();
        _navbar.Initialize(navbarRoot);

        // ── HomePage Controller ──
        // Pass the entire page body so it can find the card instances inside
        var pageBody = _root.Q<VisualElement>("page-body");
        _homePage = new HomePageController();
        _homePage.Initialize(pageBody);
    }


    // ─────────────────────────────────────────
    //  EVENT WIRING — The Mediator Pattern
    // ─────────────────────────────────────────

    /// <summary>
    /// Wires events between controllers.
    /// This is the ONLY place where controllers are connected.
    ///
    /// CROSS-DOCUMENT COMMUNICATION:
    /// The Navbar fires OnShowPopupRequested.
    /// This controller catches it and calls PopupController.Show().
    /// PopupController lives on a DIFFERENT GameObject/UIDocument.
    /// The reference was set via [SerializeField] in the Inspector.
    /// </summary>
    private void WireEvents()
    {
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
            }
            else
            {
                Debug.LogWarning("[MultiDocManager] PopupController reference not set! " +
                    "Drag the PopupOverlay GameObject into the Inspector field.");
            }
        };

        // ── Navbar → Page content ──
        _navbar.OnNavLinkClicked += (linkName) =>
        {
            Debug.Log($"[MultiDocManager] Navigation: {linkName}");
            // In a real app, you'd swap page content here
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
        };

        // ── Popup → Main page (cross-document events!) ──
        if (popupController != null)
        {
            popupController.OnConfirm += () =>
            {
                Debug.Log("[MultiDocManager] Popup confirmed — handling in main controller.");
            };

            popupController.OnCancel += () =>
            {
                Debug.Log("[MultiDocManager] Popup cancelled — handling in main controller.");
            };
        }
    }
}
