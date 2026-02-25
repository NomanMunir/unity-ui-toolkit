/*
 * ============================================
 *  PopupController.cs — Popup UIDocument Layer
 * ============================================
 *  MONOBEHAVIOUR — Because this lives on a SEPARATE GameObject
 *  with its OWN UIDocument component.
 *
 *  WHY IS THIS A MONOBEHAVIOUR (unlike NavbarController)?
 *  ─────────────────────────────────────────
 *  - It has its OWN UIDocument on a different GameObject
 *  - It needs OnEnable/OnDisable lifecycle to get rootVisualElement
 *  - The main page CAN'T Q<T>() into this document's tree
 *  - Communication happens via [SerializeField] reference + events
 *
 *  SORTING ORDER:
 *  ─────────────────────────────────────────
 *  The UIDocument on this GameObject should have sortingOrder = 100.
 *  Higher sortingOrder = renders ON TOP of lower ones.
 *  The main page has the default sortingOrder = 0.
 *
 *  SHOW/HIDE STRATEGY:
 *  ─────────────────────────────────────────
 *  We use rootVisualElement.style.display = DisplayStyle.None/Flex.
 *  This keeps the UIDocument alive but invisible — no lifecycle calls,
 *  faster toggle, and avoids duplicate event registration.
 *
 *  The GameObject stays ACTIVE at all times. Only the visual tree
 *  is hidden/shown. Events are registered ONCE in OnEnable.
 */

using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the popup overlay UIDocument layer.
/// Attach this to the popup's own GameObject (separate from the main page).
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class PopupController : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  EVENTS — For cross-document communication
    // ─────────────────────────────────────────

    /// <summary>Fired when the user clicks Confirm.</summary>
    public event Action OnConfirm;

    /// <summary>Fired when the user clicks Cancel.</summary>
    public event Action OnCancel;


    // ─────────────────────────────────────────
    //  STATE
    // ─────────────────────────────────────────
    private VisualElement _root;
    private bool _eventsRegistered;


    // ─────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────

    private void OnEnable()
    {
        // Get THIS document's root (NOT the main page's root)
        var uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        if (_root == null)
        {
            Debug.LogWarning("[PopupController] rootVisualElement is null — UIDocument may not have a Source Asset assigned.");
            return;
        }

        // Register events ONCE only
        if (!_eventsRegistered)
        {
            RegisterEvents();
            _eventsRegistered = true;
        }

        // Start hidden — the popup is invisible until Show() is called
        _root.style.display = DisplayStyle.None;

        Debug.Log($"[PopupController] Enabled. sortingOrder = {uiDoc.sortingOrder}");
    }


    // ─────────────────────────────────────────
    //  EVENT REGISTRATION
    // ─────────────────────────────────────────

    private void RegisterEvents()
    {
        // Confirm button
        var confirmBtn = _root.Q<Button>("btn-popup-confirm");
        if (confirmBtn != null)
        {
            confirmBtn.clicked += () =>
            {
                Debug.Log("[PopupController] ✔ Confirmed!");
                OnConfirm?.Invoke();
                Hide();
            };
        }

        // Cancel button
        var cancelBtn = _root.Q<Button>("btn-popup-cancel");
        if (cancelBtn != null)
        {
            cancelBtn.clicked += () =>
            {
                Debug.Log("[PopupController] ✖ Cancelled.");
                OnCancel?.Invoke();
                Hide();
            };
        }

        // Click backdrop to dismiss (optional UX pattern)
        var backdrop = _root.Q<VisualElement>("popup-backdrop");
        if (backdrop != null)
        {
            backdrop.RegisterCallback<ClickEvent>(evt =>
            {
                // Only dismiss if the click was directly on the backdrop,
                // not on the dialog inside it (event bubbling check)
                if (evt.target == backdrop)
                {
                    Debug.Log("[PopupController] Backdrop clicked — dismissing.");
                    OnCancel?.Invoke();
                    Hide();
                }
            });
        }
    }


    // ─────────────────────────────────────────
    //  SHOW / HIDE
    // ─────────────────────────────────────────

    /// <summary>
    /// Shows the popup by setting display to Flex.
    /// The UIDocument stays active — only the visual tree becomes visible.
    /// </summary>
    public void Show()
    {
        if (_root == null) return;
        _root.style.display = DisplayStyle.Flex;
        Debug.Log("[PopupController] Popup shown.");
    }

    /// <summary>
    /// Hides the popup by setting display to None.
    /// The UIDocument stays active — only the visual tree becomes invisible.
    /// </summary>
    public void Hide()
    {
        if (_root == null) return;
        _root.style.display = DisplayStyle.None;
        Debug.Log("[PopupController] Popup hidden.");
    }

    /// <summary>
    /// Updates the dialog content dynamically and shows the popup.
    /// Demonstrates modifying template instance content at runtime.
    /// </summary>
    public void ShowWithMessage(string title, string message, string icon = "⚠")
    {
        if (_root == null) return;

        // Update content
        var titleLabel = _root.Q<Label>("popup-title");
        var messageLabel = _root.Q<Label>("popup-message");
        var iconLabel = _root.Q<Label>("popup-icon");

        if (titleLabel != null) titleLabel.text = title;
        if (messageLabel != null) messageLabel.text = message;
        if (iconLabel != null) iconLabel.text = icon;

        // Show
        _root.style.display = DisplayStyle.Flex;
        Debug.Log($"[PopupController] Popup shown with: {title}");
    }
}
