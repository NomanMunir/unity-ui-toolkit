/*
 * ============================================
 *  HomePageController.cs — Page Content Logic
 * ============================================
 *  PLAIN C# CLASS (not a MonoBehaviour).
 *
 *  Demonstrates how to populate MULTIPLE INSTANCES
 *  of the same UXML component with different data.
 *
 *  KEY CONCEPT — TemplateContainer:
 *  ─────────────────────────────────────────
 *  When you use <ui:Instance template="ProfileCard" name="card-1" />,
 *  Unity wraps the component inside a TemplateContainer element.
 *
 *  To query elements INSIDE a specific instance:
 *      var card = _root.Q<TemplateContainer>("card-1");
 *      var name = card.Q<Label>("profile-name");
 *
 *  If you skip the TemplateContainer and query globally:
 *      var name = _root.Q<Label>("profile-name");
 *  You'll get the FIRST match across ALL instances — probably not what you want!
 */

using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Populates profile cards with demo data and handles card interactions.
/// </summary>
public class HomePageController
{
    // ─────────────────────────────────────────
    //  EVENTS
    // ─────────────────────────────────────────

    /// <summary>Fired when a card's "View Details" button is clicked.</summary>
    public event Action<string> OnCardDetailsClicked;


    // ─────────────────────────────────────────
    //  STATE
    // ─────────────────────────────────────────
    private VisualElement _root;


    // ─────────────────────────────────────────
    //  DEMO DATA
    // ─────────────────────────────────────────

    /// <summary>
    /// Simple struct for demo data.
    /// In a real project, this would come from a ScriptableObject or database.
    /// </summary>
    private struct TeamMember
    {
        public string Icon;
        public string Name;
        public string Badge;
        public string Role;

        public TeamMember(string icon, string name, string badge, string role)
        {
            Icon = icon;
            Name = name;
            Badge = badge;
            Role = role;
        }
    }

    private static readonly TeamMember[] DemoTeam = new[]
    {
        new TeamMember("🧙", "Alice Chen",    "LEAD",     "UI Architect — designs the overall document structure and component hierarchy"),
        new TeamMember("⚔",  "Bob Martinez",  "DEV",      "C# Developer — implements controllers, events, and cross-document communication"),
        new TeamMember("🎨", "Carol Tanaka",   "DESIGN",   "USS Stylist — creates the visual design system and transitions"),
    };


    // ─────────────────────────────────────────
    //  INITIALIZATION
    // ─────────────────────────────────────────

    /// <summary>
    /// Initializes the page content and populates cards.
    /// </summary>
    public void Initialize(VisualElement pageRoot)
    {
        _root = pageRoot;

        PopulateCards();

        Debug.Log("[HomePageController] Initialized with demo data.");
    }

    /// <summary>
    /// Populates each ProfileCard instance with data.
    ///
    /// IMPORTANT PATTERN — Querying inside TemplateContainer:
    /// ─────────────────────────────────────────
    ///   1. Find the TemplateContainer by its instance name
    ///   2. Query INSIDE that container for child elements
    ///
    /// This scopes the query to a specific card instance,
    /// preventing collisions with same-named elements in other cards.
    /// </summary>
    private void PopulateCards()
    {
        for (int i = 0; i < DemoTeam.Length; i++)
        {
            var member = DemoTeam[i];
            string instanceName = $"card-{i + 1}";

            // Step 1: Find the TemplateContainer for this card instance
            var cardContainer = _root.Q<TemplateContainer>(instanceName);
            if (cardContainer == null)
            {
                Debug.LogWarning($"[HomePageController] Card instance '{instanceName}' not found!");
                continue;
            }

            // Step 2: Query INSIDE the container to set data
            var avatarIcon = cardContainer.Q<Label>("profile-avatar-icon");
            var nameLabel  = cardContainer.Q<Label>("profile-name");
            var badgeLabel = cardContainer.Q<Label>("profile-badge-text");
            var roleLabel  = cardContainer.Q<Label>("profile-role");
            var detailsBtn = cardContainer.Q<Button>("profile-details-btn");

            // Step 3: Set the data
            if (avatarIcon != null) avatarIcon.text = member.Icon;
            if (nameLabel  != null) nameLabel.text  = member.Name;
            if (badgeLabel != null) badgeLabel.text  = member.Badge;
            if (roleLabel  != null) roleLabel.text   = member.Role;

            // Step 4: Wire events (capture member name for closure)
            if (detailsBtn != null)
            {
                string memberName = member.Name;
                detailsBtn.clicked += () =>
                {
                    Debug.Log($"[HomePageController] Details clicked for: {memberName}");
                    OnCardDetailsClicked?.Invoke(memberName);
                };
            }
        }
    }
}
