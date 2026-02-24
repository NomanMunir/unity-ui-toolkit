/*
 * ============================================
 *  NavbarController.cs — Navigation Bar Logic
 * ============================================
 *  PLAIN C# CLASS (not a MonoBehaviour).
 *
 *  WHY NOT A MONOBEHAVIOUR?
 *  ─────────────────────────────────────────
 *  This controller manages UI elements inside a
 *  Template/Instance component. It doesn't need its own
 *  GameObject or lifecycle — the master controller
 *  creates it and passes in the relevant VisualElement root.
 *
 *  This is the recommended pattern for sub-panel controllers:
 *  - Lighter than MonoBehaviours (no GameObject overhead)
 *  - Easier to test (can be instantiated in unit tests)
 *  - Master controller handles coordination between them
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles navbar interactions: link highlighting and popup trigger.
/// Receives its root element from the master controller.
/// </summary>
public class NavbarController
{
    // ─────────────────────────────────────────
    //  EVENTS — Fired for the master controller to handle
    // ─────────────────────────────────────────

    /// <summary>
    /// Fired when the user clicks the "Show Popup" button.
    /// The master controller listens and tells PopupController to show.
    /// </summary>
    public event Action OnShowPopupRequested;

    /// <summary>
    /// Fired when a nav link is clicked. Passes the link name (e.g., "HOME").
    /// </summary>
    public event Action<string> OnNavLinkClicked;


    // ─────────────────────────────────────────
    //  STATE
    // ─────────────────────────────────────────
    private VisualElement _root;
    private readonly List<Button> _navLinks = new List<Button>();
    private Button _activeLink;


    // ─────────────────────────────────────────
    //  INITIALIZATION
    // ─────────────────────────────────────────

    /// <summary>
    /// Initializes the navbar controller with the navbar's root element.
    /// Called by the master controller after the visual tree is built.
    /// </summary>
    /// <param name="navbarRoot">
    /// The VisualElement containing the navbar.
    /// This is queried from the page's visual tree.
    /// </param>
    public void Initialize(VisualElement navbarRoot)
    {
        _root = navbarRoot;

        CacheLinks();
        RegisterEvents();

        Debug.Log("[NavbarController] Initialized.");
    }

    /// <summary>
    /// Finds all nav links and stores references.
    /// </summary>
    private void CacheLinks()
    {
        // Query all buttons with the "nav-link" class
        _root.Query<Button>(className: "nav-link").ForEach(btn =>
        {
            _navLinks.Add(btn);

            // Track which link starts as active
            if (btn.ClassListContains("nav-link-active"))
            {
                _activeLink = btn;
            }
        });
    }


    // ─────────────────────────────────────────
    //  EVENT REGISTRATION
    // ─────────────────────────────────────────

    private void RegisterEvents()
    {
        // Nav links — toggle active state on click
        foreach (var link in _navLinks)
        {
            link.clicked += () => SetActiveLink(link);
        }

        // Popup button
        var popupBtn = _root.Q<Button>("btn-show-popup");
        if (popupBtn != null)
        {
            popupBtn.clicked += () =>
            {
                Debug.Log("[NavbarController] Show Popup requested.");
                OnShowPopupRequested?.Invoke();
            };
        }
    }


    // ─────────────────────────────────────────
    //  ACTIVE LINK MANAGEMENT
    // ─────────────────────────────────────────

    /// <summary>
    /// Sets the clicked link as active and removes active from the previous.
    /// This is how you toggle CSS classes from C#:
    ///   element.AddToClassList("class-name")
    ///   element.RemoveFromClassList("class-name")
    /// </summary>
    private void SetActiveLink(Button link)
    {
        // Remove active from previous
        _activeLink?.RemoveFromClassList("nav-link-active");

        // Set new active
        link.AddToClassList("nav-link-active");
        _activeLink = link;

        string linkName = link.text;
        Debug.Log($"[NavbarController] Nav link clicked: {linkName}");
        OnNavLinkClicked?.Invoke(linkName);
    }
}
