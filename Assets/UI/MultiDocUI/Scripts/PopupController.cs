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
 *  SHOW/HIDE STRATEGIES:
 *  ─────────────────────────────────────────
 *  Option A: gameObject.SetActive(true/false)
 *    - Completely removes/adds the UIDocument
 *    - Triggers OnEnable/OnDisable
 *    - Best for heavy overlays
 *
 *  Option B: rootVisualElement.style.display = DisplayStyle.None/Flex
 *    - Keeps the UIDocument alive but invisible
 *    - No lifecycle calls, faster toggle
 *    - Best for frequently toggled popups
 *
 *  We use Option A here for clarity.
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


    // ─────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────

    private void OnEnable()
    {
        // Get THIS document's root (NOT the main page's root)
        var uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        RegisterEvents();

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
    /// Shows the popup by activating this GameObject.
    /// The UIDocument becomes part of the render stack.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        Debug.Log("[PopupController] Popup shown (layer activated).");
    }

    /// <summary>
    /// Hides the popup by deactivating this GameObject.
    /// The UIDocument is completely removed from the render stack.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log("[PopupController] Popup hidden (layer deactivated).");
    }

    /// <summary>
    /// Updates the dialog content dynamically before showing.
    /// Demonstrates modifying template instance content at runtime.
    /// </summary>
    public void ShowWithMessage(string title, string message, string icon = "⚠")
    {
        // Activate first so OnEnable runs and _root is set
        gameObject.SetActive(true);

        // Now update content
        var titleLabel = _root.Q<Label>("popup-title");
        var messageLabel = _root.Q<Label>("popup-message");
        var iconLabel = _root.Q<Label>("popup-icon");

        if (titleLabel != null) titleLabel.text = title;
        if (messageLabel != null) messageLabel.text = message;
        if (iconLabel != null) iconLabel.text = icon;

        Debug.Log($"[PopupController] Popup shown with: {title}");
    }
}
