/*
 * ============================================
 *  ChatPanelController.cs — Chat / Log Panel
 * ============================================
 *  MONOBEHAVIOUR — Owns its own UIDocument (sortingOrder = 1).
 *
 *  NEW CONCEPT — DYNAMIC ELEMENT CREATION:
 *  ─────────────────────────────────────────
 *  Unlike ProfileCards (which use Template/Instance in UXML),
 *  chat messages are created ENTIRELY from C# at runtime.
 *
 *  This demonstrates the other approach to building UI:
 *  - Template/Instance: static composition (known at design time)
 *  - C# element creation: dynamic content (unknown count/content)
 *
 *  Each message is built like this:
 *      var msg = new VisualElement();
 *      msg.AddToClassList("chat-message");
 *      var text = new Label("Hello!");
 *      msg.Add(text);
 *      scrollView.Add(msg);
 *
 *  CROSS-DOCUMENT COMMUNICATION:
 *  ─────────────────────────────────────────
 *  Other controllers can log messages to this panel via:
 *      chatPanel.AddMessage("System", "Player connected", MessageType.System);
 *
 *  The manager wires events so that actions on the main page
 *  (nav clicks, card clicks, popup confirm/cancel) automatically
 *  log entries here — demonstrating real cross-document data flow.
 */

using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls the chat/log panel UIDocument layer.
/// Attach to its own GameObject with UIDocument (sortingOrder = 1).
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ChatPanelController : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  MESSAGE TYPES
    // ─────────────────────────────────────────

    /// <summary>
    /// Determines the color/icon of the message.
    /// Each type maps to a USS class (e.g., chat-msg-system).
    /// </summary>
    public enum MessageType
    {
        System,     // Blue — system info
        Event,      // Yellow — UI events
        User,       // Purple — user actions
        Success,    // Green — confirmations
        Error       // Red — errors / cancels
    }


    // ─────────────────────────────────────────
    //  EVENTS
    // ─────────────────────────────────────────

    /// <summary>Fired when the user sends a message via the input bar.</summary>
    public event Action<string> OnMessageSent;

    /// <summary>Fired when the close button is clicked.</summary>
    public event Action OnCloseRequested;


    // ─────────────────────────────────────────
    //  STATE
    // ─────────────────────────────────────────
    private VisualElement _root;
    private ScrollView _messageList;
    private TextField _inputField;
    private Label _badgeCount;
    private int _messageCount;
    private bool _eventsRegistered;


    // ─────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        _root = uiDoc.rootVisualElement;

        if (_root == null)
        {
            Debug.LogWarning("[ChatPanelController] rootVisualElement is null.");
            return;
        }

        // Cache references
        _messageList = _root.Q<ScrollView>("chat-messages");
        _inputField = _root.Q<TextField>("chat-input");
        _badgeCount = _root.Q<Label>("chat-badge-count");

        if (!_eventsRegistered)
        {
            RegisterEvents();
            _eventsRegistered = true;
        }

        // Start hidden
        _root.style.display = DisplayStyle.None;

        Debug.Log($"[ChatPanelController] Enabled. sortingOrder = {uiDoc.sortingOrder}");
    }


    // ─────────────────────────────────────────
    //  EVENT REGISTRATION
    // ─────────────────────────────────────────

    private void RegisterEvents()
    {
        // Close button
        var closeBtn = _root.Q<Button>("btn-chat-close");
        if (closeBtn != null)
        {
            closeBtn.clicked += () =>
            {
                Hide();
                OnCloseRequested?.Invoke();
            };
        }

        // Send button
        var sendBtn = _root.Q<Button>("btn-chat-send");
        if (sendBtn != null)
        {
            sendBtn.clicked += SendCurrentMessage;
        }

        // Enter key on input field
        if (_inputField != null)
        {
            _inputField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    SendCurrentMessage();
                    evt.StopPropagation();
                }
            });
        }
    }

    private void SendCurrentMessage()
    {
        if (_inputField == null) return;

        string text = _inputField.value?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        // Add the user's message to the log
        AddMessage("You", text, MessageType.User);

        // Clear input
        _inputField.value = "";
        _inputField.Focus();

        // Notify listeners
        OnMessageSent?.Invoke(text);
    }


    // ─────────────────────────────────────────
    //  PUBLIC API — Adding Messages
    // ─────────────────────────────────────────

    /// <summary>
    /// Adds a message to the chat log.
    /// Called by other controllers via the manager.
    ///
    /// DYNAMIC ELEMENT CREATION:
    /// ─────────────────────────────────────────
    /// Instead of using UXML templates, we build the UI
    /// entirely from C#. This is the right approach when
    /// the number of elements is unknown at design time.
    ///
    ///   new VisualElement()  → creates a div-like container
    ///   .AddToClassList()    → applies USS classes for styling
    ///   .Add()               → appends a child element
    /// </summary>
    public void AddMessage(string sender, string text, MessageType type)
    {
        if (_messageList == null) return;

        // ── Build the message element tree from C# ──

        // Root container:  <div class="chat-message chat-msg-{type}">
        var message = new VisualElement();
        message.AddToClassList("chat-message");
        message.AddToClassList($"chat-msg-{type.ToString().ToLower()}");

        // Icon:  <span class="chat-message-icon">🔹</span>
        var icon = new Label(GetIconForType(type));
        icon.AddToClassList("chat-message-icon");
        message.Add(icon);

        // Body container:  <div class="chat-message-body">
        var body = new VisualElement();
        body.AddToClassList("chat-message-body");

        // Sender:  <span class="chat-message-sender">System</span>
        var senderLabel = new Label(sender);
        senderLabel.AddToClassList("chat-message-sender");
        body.Add(senderLabel);

        // Text:  <span class="chat-message-text">Message here</span>
        var textLabel = new Label(text);
        textLabel.AddToClassList("chat-message-text");
        body.Add(textLabel);

        message.Add(body);

        // Timestamp:  <span class="chat-message-time">12:34</span>
        var time = new Label(DateTime.Now.ToString("HH:mm"));
        time.AddToClassList("chat-message-time");
        message.Add(time);

        // Add to scroll view
        _messageList.Add(message);

        // Update badge count
        _messageCount++;
        if (_badgeCount != null)
        {
            _badgeCount.text = _messageCount.ToString();
        }

        // Auto-scroll to bottom (schedule to next frame so layout updates first)
        _messageList.schedule.Execute(() =>
        {
            _messageList.scrollOffset = new Vector2(0, float.MaxValue);
        });
    }


    // ─────────────────────────────────────────
    //  SHOW / HIDE
    // ─────────────────────────────────────────

    public void Show()
    {
        if (_root == null) return;
        _root.style.display = DisplayStyle.Flex;
        Debug.Log("[ChatPanelController] Chat panel shown.");
    }

    public void Hide()
    {
        if (_root == null) return;
        _root.style.display = DisplayStyle.None;
        Debug.Log("[ChatPanelController] Chat panel hidden.");
    }

    public void Toggle()
    {
        if (_root == null) return;

        bool isVisible = _root.resolvedStyle.display == DisplayStyle.Flex;
        if (isVisible)
            Hide();
        else
            Show();
    }

    public bool IsVisible => _root != null && _root.resolvedStyle.display == DisplayStyle.Flex;


    // ─────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────

    private static string GetIconForType(MessageType type)
    {
        return type switch
        {
            MessageType.System  => "ℹ",
            MessageType.Event   => "⚡",
            MessageType.User    => "►",
            MessageType.Success => "✔",
            MessageType.Error   => "✖",
            _                   => "•"
        };
    }
}
