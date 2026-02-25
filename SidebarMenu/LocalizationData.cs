/*
 * ============================================
 *  LocalizationData.cs — Simple Localization System
 * ============================================
 *
 *  CONCEPT: Data Binding & Localization
 *
 *  In UI Toolkit, "data binding" means connecting your UI
 *  elements to data sources so they update automatically.
 *
 *  This simple system demonstrates the pattern:
 *  1. Store all strings in a dictionary per language
 *  2. Each string has a KEY (e.g., "header.title")
 *  3. UI labels reference keys, not hardcoded text
 *  4. When the language changes, all labels update
 *
 *  In production, you'd use Unity's Localization package
 *  (com.unity.localization) for full locale support,
 *  string tables, and automatic binding. This is a
 *  simplified educational version.
 *
 *  DATA BINDING PATTERN:
 *    Label → binds to → key "header.title"
 *    LocalizationData.Get("header.title") → returns translated string
 *    On language change → all bound labels call Get() again
 */

using System.Collections.Generic;

/// <summary>
/// Simple localization system that maps string keys
/// to translated values for multiple languages.
///
/// USAGE:
///   LocalizationData.SetLanguage("ar");
///   string text = LocalizationData.Get("header.title");
/// </summary>
public static class LocalizationData
{
    // ─── Supported Languages ───
    public static readonly string[] SupportedLanguages = { "en", "ar", "fr" };
    public static readonly string[] LanguageNames = { "English", "العربية", "Français" };

    private static string _currentLanguage = "en";

    /// <summary>Gets the currently active language code.</summary>
    public static string CurrentLanguage => _currentLanguage;

    // ─────────────────────────────────────────
    //  STRING TABLES — One dictionary per language
    // ─────────────────────────────────────────

    /*
     * DATA STRUCTURE:
     * Dictionary<languageCode, Dictionary<key, translatedString>>
     *
     * Keys use dot notation for organization:
     *   "header.title"     → Header section, title label
     *   "tab.all"          → Tab section, "All" button
     *   "search.placeholder" → Search bar placeholder
     */
    private static readonly Dictionary<string, Dictionary<string, string>> _strings = new()
    {
        // ── ENGLISH ──
        ["en"] = new Dictionary<string, string>
        {
            ["header.title"]       = "Sim Assets",
            ["header.subtitle"]    = "Browse and configure simulation entities",
            ["search.placeholder"] = "Search assets...",
            ["tab.all"]            = "All",
            ["tab.air"]            = "Air",
            ["tab.land"]           = "Land",
            ["tab.sea"]            = "Sea",
            ["results.count"]      = "{0} asset(s) found",
            ["footer.text"]        = "UI Toolkit • Sidebar Demo",
            ["theme.tooltip"]      = "Toggle theme",
            ["lang.tooltip"]       = "Change language",
            ["empty.title"]        = "No assets found",
            ["empty.hint"]         = "Try a different search or category",
        },

        // ── ARABIC ──
        ["ar"] = new Dictionary<string, string>
        {
            ["header.title"]       = "أصول المحاكاة",
            ["header.subtitle"]    = "تصفح وتكوين كيانات المحاكاة",
            ["search.placeholder"] = "ابحث عن الأصول...",
            ["tab.all"]            = "الكل",
            ["tab.air"]            = "جوي",
            ["tab.land"]           = "بري",
            ["tab.sea"]            = "بحري",
            ["results.count"]      = "تم العثور على {0} أصل",
            ["footer.text"]        = "واجهة المستخدم • عرض الشريط الجانبي",
            ["theme.tooltip"]      = "تبديل السمة",
            ["lang.tooltip"]       = "تغيير اللغة",
            ["empty.title"]        = "لم يتم العثور على أصول",
            ["empty.hint"]         = "جرّب بحثاً أو فئة مختلفة",
        },

        // ── FRENCH ──
        ["fr"] = new Dictionary<string, string>
        {
            ["header.title"]       = "Actifs Sim",
            ["header.subtitle"]    = "Parcourir et configurer les entités",
            ["search.placeholder"] = "Rechercher des actifs...",
            ["tab.all"]            = "Tous",
            ["tab.air"]            = "Air",
            ["tab.land"]           = "Terre",
            ["tab.sea"]            = "Mer",
            ["results.count"]      = "{0} actif(s) trouvé(s)",
            ["footer.text"]        = "UI Toolkit • Démo barre latérale",
            ["theme.tooltip"]      = "Changer le thème",
            ["lang.tooltip"]       = "Changer de langue",
            ["empty.title"]        = "Aucun actif trouvé",
            ["empty.hint"]         = "Essayez une autre recherche ou catégorie",
        }
    };


    // ─────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────

    /// <summary>
    /// Sets the active language. All subsequent Get() calls
    /// will return strings in this language.
    /// </summary>
    public static void SetLanguage(string langCode)
    {
        if (_strings.ContainsKey(langCode))
            _currentLanguage = langCode;
    }

    /// <summary>
    /// Gets a localized string by key.
    /// Falls back to English if key not found in current language.
    /// Falls back to the key itself if not found at all.
    ///
    /// This is the BINDING function — UI elements call this
    /// to get their display text.
    /// </summary>
    public static string Get(string key)
    {
        // Try current language
        if (_strings.TryGetValue(_currentLanguage, out var table) &&
            table.TryGetValue(key, out var value))
            return value;

        // Fallback to English
        if (_strings.TryGetValue("en", out var enTable) &&
            enTable.TryGetValue(key, out var enValue))
            return enValue;

        // Last resort: return the key itself
        return $"[{key}]";
    }

    /// <summary>
    /// Format overload — replaces {0}, {1}, etc. with args.
    /// Example: Get("results.count", 5) → "5 asset(s) found"
    /// </summary>
    public static string Get(string key, params object[] args)
    {
        return string.Format(Get(key), args);
    }

    /// <summary>Gets the index of the current language in SupportedLanguages.</summary>
    public static int GetCurrentLanguageIndex()
    {
        for (int i = 0; i < SupportedLanguages.Length; i++)
        {
            if (SupportedLanguages[i] == _currentLanguage) return i;
        }
        return 0;
    }
}
