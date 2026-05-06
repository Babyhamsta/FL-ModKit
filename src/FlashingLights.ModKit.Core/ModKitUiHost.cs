using MelonLoader.Utils;
using System.Globalization;
using UnityEngine;

namespace FlashingLights.ModKit.Core;

public static class ModKitUiHost
{
    private const int MinimumSidebarWidth = 280;
    private const int MinimumDetailsWidth = 360;
    private const int Gutter = 14;

    private static readonly Dictionary<string, string> DraftValues = new(StringComparer.OrdinalIgnoreCase);
    private static Vector2 modScroll;
    private static Vector2 configScroll;
    private static Vector2 aboutScroll;
    private static Vector2 logsScroll;
    private static Vector2 logViewerScroll;
    private static string modFilterText = string.Empty;
    private static string? selectedModId;
    private static string? selectedLogPath;
    private static string logViewerText = string.Empty;
    private static string? openDropdownKey;
    private static string status = ModKitStrings.Get("ui.insertCloses");
    private static string? pendingResetModId;
    private static DateTimeOffset pendingResetUntil = DateTimeOffset.MinValue;
    private static string? pendingRebindModId;
    private static string? pendingRebindName;
    private static ModKitUiTab selectedTab = ModKitUiTab.Config;
    private static int lastUpdateFrame = -1;
    private static CursorState? capturedCursorState;
    private static CursorState? pendingCursorRestoreState;
    private static int pendingCursorRestoreFrames;
    private static Texture2D? overlayBackgroundTexture;
    private static GUIStyle? headerStyle;
    private static GUIStyle? panelStyle;
    private static GUIStyle? panelHeaderStyle;
    private static GUIStyle? selectedButtonStyle;
    private static GUIStyle? buttonStyle;
    private static GUIStyle? modRowStyle;
    private static GUIStyle? selectedModRowStyle;
    private static GUIStyle? modRowButtonStyle;
    private static GUIStyle? labelStyle;
    private static GUIStyle? propertyLabelStyle;
    private static GUIStyle? mutedStyle;
    private static GUIStyle? valueStyle;
    private static GUIStyle? textFieldStyle;
    private static GUIStyle? textAreaStyle;
    private static GUIStyle? footerStyle;
    private static GUIStyle? typeBadgeStyle;
    private static GUIStyle? switchLabelStyle;

    public static bool IsVisible { get; private set; }

    private static string Text(string key, params object?[] args) => ModKitStrings.Get(key, args);

    public static void Update()
    {
        if (Time.frameCount == lastUpdateFrame)
        {
            return;
        }

        lastUpdateFrame = Time.frameCount;

        if (Input.GetKeyDown(KeyCode.Insert) || (IsVisible && Input.GetKeyDown(KeyCode.Escape)))
        {
            SetVisible(!IsVisible);
            ModKitUiInputPolicy.SuppressGameInput();
        }

        if (IsVisible)
        {
            EnsureCursor();
            ModKitUiEventSystemBlocker.SetBlocked(blocked: true);
        }
        else
        {
            ModKitUiEventSystemBlocker.SetBlocked(blocked: false);
            RestorePendingCursorState();
        }
    }

    public static void OnGUI()
    {
        if (!IsVisible)
        {
            return;
        }

        var previousDepth = GUI.depth;
        var currentEvent = Event.current;
        try
        {
            if (currentEvent != null && HandlePendingHotkeyRebind(currentEvent))
            {
                return;
            }

            if (currentEvent != null && ModKitUiInputPolicy.ShouldCloseOverlay(currentEvent.type, currentEvent.keyCode))
            {
                SetVisible(visible: false);
                currentEvent.Use();
                ModKitUiInputPolicy.SuppressGameInput();
                return;
            }

            GUI.depth = ModKitUiInputPolicy.OverlayGuiDepth;
            EnsureStyles();
            EnsureSelection();

            var screen = new Rect(0, 0, Screen.width, Screen.height);
            DrawRect(screen);
            var headerRect = new Rect(0, 0, Screen.width, ModKitUiLayoutPolicy.HeaderHeight);
            var bodyRect = new Rect(
                0,
                ModKitUiLayoutPolicy.HeaderHeight + ModKitUiLayoutPolicy.VerticalGap,
                Screen.width,
                ModKitUiLayoutPolicy.CalculateBodyHeight(Screen.height));
            var footerRect = new Rect(0, Screen.height - ModKitUiLayoutPolicy.FooterHeight, Screen.width, ModKitUiLayoutPolicy.FooterHeight);

            GUILayout.BeginArea(headerRect);
            DrawHeader();
            GUILayout.EndArea();

            GUILayout.BeginArea(bodyRect);
            DrawBody((int)bodyRect.height);
            GUILayout.EndArea();

            GUILayout.BeginArea(footerRect);
            DrawFooter();
            GUILayout.EndArea();

            if (currentEvent != null && ModKitUiInputPolicy.ShouldBlockEvent(currentEvent.type))
            {
                currentEvent.Use();
                ModKitUiInputPolicy.SuppressGameInput();
            }
        }
        finally
        {
            GUI.depth = previousDepth;
        }
    }

    public static void SetVisible(bool visible)
    {
        if (IsVisible == visible)
        {
            return;
        }

        IsVisible = visible;
        if (visible)
        {
            CaptureCursor();
            ModKitUiEventSystemBlocker.SetBlocked(blocked: true);
            status = ModKitStrings.Get("ui.opened");
        }
        else
        {
            ModKitUiEventSystemBlocker.SetBlocked(blocked: false);
            RestoreCursor();
        }
    }

    private static void DrawHeader()
    {
        GUILayout.BeginHorizontal(headerStyle!, GUILayout.Height(ModKitUiLayoutPolicy.HeaderHeight));
        GUILayout.BeginVertical(GUILayout.Width(360));
        GUILayout.Label(Text("ui.title"), labelStyle!);
        GUILayout.Label(Text("ui.subtitle", SdkInfo.Version), mutedStyle!);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();

        var tabs = new[] { ModKitUiTab.Mods, ModKitUiTab.Config, ModKitUiTab.Logs, ModKitUiTab.About };
        foreach (var tab in tabs)
        {
            var style = selectedTab == tab ? selectedButtonStyle! : buttonStyle!;
            if (GUILayout.Button(TabLabel(tab), style, GUILayout.Width(86), GUILayout.Height(32)))
            {
                selectedTab = tab;
            }
        }

        GUILayout.EndHorizontal();
    }

    private static string TabLabel(ModKitUiTab tab) => tab switch
    {
        ModKitUiTab.Mods => Text("ui.tab.mods"),
        ModKitUiTab.Config => Text("ui.tab.config"),
        ModKitUiTab.Logs => Text("ui.tab.logs"),
        ModKitUiTab.About => Text("ui.tab.about"),
        _ => tab.ToString()
    };

    private static void DrawBody(int bodyHeight)
    {
        var sidebarWidth = Math.Max(MinimumSidebarWidth, Math.Min(340, Screen.width / 4));
        var detailsWidth = Math.Max(MinimumDetailsWidth, Math.Min(520, Screen.width / 3));
        var panes = ModKitUiLayoutPolicy.GetVisiblePanes(selectedTab);

        GUILayout.BeginHorizontal(GUILayout.Height(bodyHeight));
        if (panes.HasFlag(ModKitUiPane.Mods))
        {
            DrawModsPane(selectedTab == ModKitUiTab.Mods ? detailsWidth : sidebarWidth, bodyHeight);
        }

        if (panes.HasFlag(ModKitUiPane.SelectedMod))
        {
            GUILayout.Space(Gutter);
            DrawSelectedModPane(bodyHeight);
        }

        if (panes.HasFlag(ModKitUiPane.Config))
        {
            GUILayout.Space(Gutter);
            DrawConfigPane(bodyHeight);
        }

        if (panes.HasFlag(ModKitUiPane.Logs))
        {
            DrawLogsPane(bodyHeight);
        }

        if (panes.HasFlag(ModKitUiPane.About))
        {
            DrawAboutPane(bodyHeight, expandWidth: true);
        }

        GUILayout.EndHorizontal();
    }

    private static void DrawModsPane(int width, int height)
    {
        GUILayout.BeginVertical(panelStyle!, GUILayout.Width(width), GUILayout.Height(height));
        var mods = ModKitRegistry.ManagedMods;
        var visibleMods = ModKitUiModListPolicy.FilterMods(mods, modFilterText);
        var showGroups = ModKitUiModListPolicy.ShouldShowCategoryHeaders(visibleMods);
        var orderedMods = showGroups
            ? visibleMods
                .OrderBy(mod => ModKitUiModListPolicy.CategoryFor(mod.Metadata), StringComparer.OrdinalIgnoreCase)
                .ThenBy(mod => mod.Metadata.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : visibleMods;

        GUILayout.Label(Text("ui.loadedMods", mods.Count), panelHeaderStyle!);
        modFilterText = GUILayout.TextField(modFilterText, textFieldStyle!, GUILayout.Height(28));
        modScroll = GUILayout.BeginScrollView(modScroll);

        string? currentCategory = null;
        foreach (var mod in orderedMods)
        {
            if (showGroups)
            {
                var category = ModKitUiModListPolicy.CategoryFor(mod.Metadata);
                if (!string.Equals(category, currentCategory, StringComparison.OrdinalIgnoreCase))
                {
                    currentCategory = category;
                    GUILayout.Label(category, mutedStyle!, GUILayout.Height(20));
                }
            }

            var isSelected = selectedModId != null
                && selectedModId.Equals(mod.Metadata.Id, StringComparison.OrdinalIgnoreCase);
            var rowStyle = isSelected ? selectedModRowStyle! : modRowStyle!;
            var switchLocked = !mod.IsEnabled && !ModKitMultiplayerGuard.CanEnableMods();

            GUILayout.BeginHorizontal(rowStyle, GUILayout.Height(56));
            GUILayout.BeginVertical(GUILayout.Width(58), GUILayout.Height(46));
            GUILayout.FlexibleSpace();
            var enabled = DrawSwitch(mod.IsEnabled, width: 50, disabled: switchLocked);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            if (enabled != mod.IsEnabled)
            {
                ApplyModEnabled(mod, enabled);
            }

            if (GUILayout.Button($"{mod.Metadata.DisplayName}\n{mod.Metadata.Id}", modRowButtonStyle!, GUILayout.Height(44), GUILayout.ExpandWidth(true)))
            {
                SelectMod(mod.Metadata.Id);
            }
            GUILayout.EndHorizontal();
        }

        if (mods.Count == 0)
        {
            GUILayout.Label(Text("ui.noManagedMods"), mutedStyle!);
        }
        else if (orderedMods.Count == 0)
        {
            GUILayout.Label(Text("ui.noFilterMatches"), mutedStyle!);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private static void DrawConfigPane(int height)
    {
        GUILayout.BeginVertical(panelStyle!, GUILayout.ExpandWidth(true), GUILayout.Height(height));

        if (!TryGetSelectedMod(out var mod) || mod == null)
        {
            GUILayout.Label(Text("ui.selectManagedMod"), panelHeaderStyle!);
            GUILayout.EndVertical();
            return;
        }

        GUILayout.Label(Text("ui.configTitle", mod.Metadata.DisplayName), panelHeaderStyle!);
        GUILayout.Label(mod.ConfigPath, mutedStyle!, GUILayout.Height(22));

        var adapter = mod.ConfigAdapter;
        if (adapter == null)
        {
            GUILayout.Label(Text("ui.noConfigAdapter"), mutedStyle!);
            GUILayout.EndVertical();
            return;
        }

        configScroll = GUILayout.BeginScrollView(configScroll);
        foreach (var property in adapter.GetProperties())
        {
            DrawConfigProperty(mod, adapter, property);
        }
        DrawHotkeysSection(mod);

        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal(GUILayout.Height(40));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(Text("ui.reloadFromFile"), buttonStyle!, GUILayout.Width(150), GUILayout.Height(32)))
        {
            adapter.Reload();
            ClearDraftsForMod(mod.Metadata.Id);
            status = Text("ui.configReloaded", mod.Metadata.DisplayName);
        }

        var resetPending = IsResetPending(mod.Metadata.Id);
        var resetLabel = resetPending ? Text("ui.confirmReset") : Text("ui.resetDefaults");
        if (GUILayout.Button(resetLabel, resetPending ? selectedButtonStyle! : buttonStyle!, GUILayout.Width(130), GUILayout.Height(32)))
        {
            if (resetPending)
            {
                adapter.ResetDefaults();
                ClearDraftsForMod(mod.Metadata.Id);
                pendingResetModId = null;
                status = Text("ui.configReset", mod.Metadata.DisplayName);
            }
            else
            {
                pendingResetModId = mod.Metadata.Id;
                pendingResetUntil = DateTimeOffset.UtcNow.AddSeconds(5);
                status = Text("ui.resetPrompt", mod.Metadata.DisplayName);
            }
        }

        if (GUILayout.Button(Text("ui.saveConfig"), selectedButtonStyle!, GUILayout.Width(120), GUILayout.Height(32)))
        {
            SaveDrafts(mod, adapter);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private static void DrawConfigProperty(IModKitManagedMod mod, IModKitConfigAdapter adapter, ModKitConfigProperty property)
    {
        var modId = mod.Metadata.Id;
        var draftKey = DraftKey(modId, property.Name);
        if (!DraftValues.TryGetValue(draftKey, out var draft))
        {
            draft = property.ValueText;
            DraftValues[draftKey] = draft;
        }

        var rowHeight = GetConfigPropertyRowHeight(property, draftKey);
        GUILayout.BeginHorizontal(GUILayout.Height(rowHeight));
        GUILayout.Label(property.DisplayName, propertyLabelStyle!, GUILayout.Width(210));

        if (!property.CanWrite)
        {
            GUILayout.Label(property.ValueText, mutedStyle!, GUILayout.ExpandWidth(true), GUILayout.Height(28));
            GUILayout.Label(Text("ui.readOnly"), typeBadgeStyle!, GUILayout.Width(72), GUILayout.Height(24));
            GUILayout.EndHorizontal();
            return;
        }

        if (property.Kind == ModKitConfigValueKind.Boolean)
        {
            var current = bool.TryParse(draft, out var parsed) && parsed;
            var lockEnable = IsEnabledProperty(property) && !current && !ModKitMultiplayerGuard.CanEnableMods();
            var changed = DrawSwitch(current, width: 58, disabled: lockEnable);
            if (changed != current)
            {
                if (IsEnabledProperty(property) && changed && !ModKitMultiplayerGuard.CanEnableMods())
                {
                    status = ModKitMultiplayerPolicy.ToggleBlockedMessage;
                    changed = current;
                }
                else if (IsEnabledProperty(property) && ShouldBlockEnabledConfigEdit(mod, changed))
                {
                    status = ModKitUiAboutPolicy.FormatToggleBlockedMessage(mod.Metadata.DisplayName);
                    changed = current;
                }
                else if (IsEnabledProperty(property))
                {
                    ApplyModEnabled(mod, changed);
                }

                var text = changed.ToString().ToLowerInvariant();
                DraftValues[draftKey] = text;
                if (!IsEnabledProperty(property) && !adapter.TrySetProperty(property.Name, text, out var error))
                {
                    status = error;
                }
            }

            GUILayout.Label(changed ? Text("ui.enabled") : Text("ui.disabled"), valueStyle!, GUILayout.Width(90), GUILayout.Height(24));
        }
        else if (property.Options.Count > 0)
        {
            DrawOptionsDropdown(adapter, property, draftKey, draft);
        }
        else if (property.Kind == ModKitConfigValueKind.StringArray)
        {
            var next = GUILayout.TextArea(draft, textAreaStyle!, GUILayout.Height(74), GUILayout.ExpandWidth(true));
            DraftValues[draftKey] = next;
            GUILayout.Label(Text("ui.array"), typeBadgeStyle!, GUILayout.Width(72), GUILayout.Height(24));
        }
        else if (IsRangedNumber(property))
        {
            DrawRangedNumberProperty(adapter, property, draftKey, draft);
        }
        else
        {
            var next = GUILayout.TextField(draft, textFieldStyle!, GUILayout.Height(26), GUILayout.ExpandWidth(true));
            DraftValues[draftKey] = next;
            GUILayout.Label(property.Kind.ToString(), typeBadgeStyle!, GUILayout.Width(72), GUILayout.Height(24));
        }

        GUILayout.EndHorizontal();
    }

    private static int GetConfigPropertyRowHeight(ModKitConfigProperty property, string draftKey)
    {
        if (property.Kind == ModKitConfigValueKind.StringArray)
        {
            return 82;
        }

        if (property.Options.Count > 0 && string.Equals(openDropdownKey, draftKey, StringComparison.Ordinal))
        {
            return 36 + Math.Min(property.Options.Count, 6) * 28;
        }

        return 34;
    }

    private static void DrawHotkeysSection(IModKitManagedMod mod)
    {
        var bindings = mod.GetHotkeyBindings();
        if (bindings == null || bindings.Count == 0)
        {
            return;
        }

        GUILayout.Space(12);
        GUILayout.Label(Text("ui.hotkeys"), panelHeaderStyle!);
        foreach (var binding in bindings)
        {
            var isPending = string.Equals(pendingRebindModId, mod.Metadata.Id, StringComparison.OrdinalIgnoreCase)
                && string.Equals(pendingRebindName, binding.Name, StringComparison.OrdinalIgnoreCase);

            GUILayout.BeginHorizontal(GUILayout.Height(34));
            GUILayout.Label(binding.DisplayName, propertyLabelStyle!, GUILayout.Width(210));
            GUILayout.Label(binding.Name, mutedStyle!, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(isPending ? Text("ui.pressKey") : binding.Key.ToString(), isPending ? selectedButtonStyle! : buttonStyle!, GUILayout.Width(130), GUILayout.Height(28)))
            {
                pendingRebindModId = mod.Metadata.Id;
                pendingRebindName = binding.Name;
                status = Text("ui.hotkeyPrompt", binding.DisplayName);
            }
            GUILayout.EndHorizontal();
        }
    }

    private static void DrawOptionsDropdown(
        IModKitConfigAdapter adapter,
        ModKitConfigProperty property,
        string draftKey,
        string draft)
    {
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        var current = string.IsNullOrWhiteSpace(draft) ? property.ValueText : draft;
        if (GUILayout.Button(current, selectedButtonStyle!, GUILayout.Height(26), GUILayout.ExpandWidth(true)))
        {
            openDropdownKey = string.Equals(openDropdownKey, draftKey, StringComparison.Ordinal)
                ? null
                : draftKey;
        }

        if (string.Equals(openDropdownKey, draftKey, StringComparison.Ordinal))
        {
            foreach (var option in property.Options)
            {
                if (GUILayout.Button(option, option == current ? selectedButtonStyle! : buttonStyle!, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                {
                    DraftValues[draftKey] = option;
                    openDropdownKey = null;
                    if (!adapter.TrySetProperty(property.Name, option, out var error))
                    {
                        status = error;
                    }
                }
            }
        }

        GUILayout.EndVertical();
        GUILayout.Label(Text("ui.option"), typeBadgeStyle!, GUILayout.Width(72), GUILayout.Height(24));
    }

    private static void DrawRangedNumberProperty(
        IModKitConfigAdapter adapter,
        ModKitConfigProperty property,
        string draftKey,
        string draft)
    {
        var min = property.Minimum!.Value;
        var max = property.Maximum!.Value;
        var current = TryParseDouble(draft, out var draftValue)
            ? draftValue
            : TryParseDouble(property.ValueText, out var liveValue)
                ? liveValue
                : min;
        current = Math.Clamp(current, min, max);

        var rawSlider = GUILayout.HorizontalSlider(
            (float)current,
            (float)min,
            (float)max,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(24));
        var snappedSlider = SnapToStep(rawSlider, min, property.Step.GetValueOrDefault());
        snappedSlider = Math.Clamp(snappedSlider, min, max);

        if (Math.Abs(snappedSlider - current) > 0.0001)
        {
            var sliderText = FormatNumber(snappedSlider, property.Kind);
            DraftValues[draftKey] = sliderText;
            if (!adapter.TrySetProperty(property.Name, sliderText, out var error))
            {
                status = error;
            }

            draft = sliderText;
        }

        var next = GUILayout.TextField(draft, textFieldStyle!, GUILayout.Width(82), GUILayout.Height(26));
        DraftValues[draftKey] = next;
        GUILayout.Label(property.Kind.ToString(), typeBadgeStyle!, GUILayout.Width(72), GUILayout.Height(24));
    }

    private static bool IsRangedNumber(ModKitConfigProperty property)
    {
        return property.HasRange
            && (property.Kind == ModKitConfigValueKind.Integer || property.Kind == ModKitConfigValueKind.Float);
    }

    private static bool TryParseDouble(string value, out double parsed)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
    }

    private static double SnapToStep(double value, double minimum, double step)
    {
        if (step <= 0)
        {
            return value;
        }

        return minimum + Math.Round((value - minimum) / step) * step;
    }

    private static string FormatNumber(double value, ModKitConfigValueKind kind)
    {
        if (kind == ModKitConfigValueKind.Integer)
        {
            return Convert.ToInt64(Math.Round(value)).ToString(CultureInfo.InvariantCulture);
        }

        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static void DrawSelectedModPane(int height)
    {
        GUILayout.BeginVertical(panelStyle!, GUILayout.ExpandWidth(true), GUILayout.Height(height));
        if (!TryGetSelectedMod(out var mod) || mod == null)
        {
            GUILayout.Label(Text("ui.selectedMod"), panelHeaderStyle!);
            GUILayout.Label(Text("ui.selectManagedMod"), mutedStyle!);
            GUILayout.EndVertical();
            return;
        }

        GUILayout.Label(mod.Metadata.DisplayName, panelHeaderStyle!);
        DrawInfoRow(Text("ui.info.id"), mod.Metadata.Id);
        DrawInfoRow(Text("ui.info.version"), mod.Metadata.Version);
        DrawInfoRow(Text("ui.info.author"), mod.Metadata.Author);
        DrawInfoRow(Text("ui.info.license"), ModKitUiAboutPolicy.FormatLicense(mod.Metadata.License));
        DrawInfoRow(Text("ui.info.minSdk"), ModKitUiAboutPolicy.FormatMinSdkVersion(mod.Metadata.MinSdkVersion));
        DrawInfoRow(Text("ui.info.category"), ModKitUiModListPolicy.CategoryFor(mod.Metadata));
        DrawInfoRow(Text("ui.info.tags"), mod.Metadata.Tags.Count == 0 ? Text("ui.none") : string.Join(", ", mod.Metadata.Tags));
        DrawInfoRow(Text("ui.info.dependencies"), ModKitUiAboutPolicy.FormatDependencies(mod.Metadata.Dependencies));
        var modValidation = ModKitRegistry.GetValidation(mod.Metadata.Id);
        DrawInfoRow(Text("ui.info.status"), ModKitUiAboutPolicy.FormatValidationStatus(modValidation));
        DrawInfoRow(Text("ui.info.state"), mod.IsEnabled ? Text("ui.enabled") : Text("ui.disabled"));
        if (!modValidation.IsValid)
        {
            GUILayout.Space(4);
            foreach (var issue in modValidation.Issues)
            {
                GUILayout.Label("• " + issue, mutedStyle!);
            }
        }
        GUILayout.Space(8);
        if (!string.IsNullOrWhiteSpace(mod.Metadata.Changelog))
        {
            GUILayout.Label(mod.Metadata.Changelog, mutedStyle!);
        }

        GUILayout.Space(12);
        if (GUILayout.Button(Text("ui.openConfig"), selectedButtonStyle!, GUILayout.Width(140), GUILayout.Height(32)))
        {
            selectedTab = ModKitUiTab.Config;
        }
        if (GUILayout.Button(Text("ui.rerunSelfCheck"), buttonStyle!, GUILayout.Width(160), GUILayout.Height(32)))
        {
            var validation = ModKitRegistry.RefreshSelfCheck(mod.Metadata.Id);
            status = validation.IsValid
                ? Text("ui.selfCheckOk", mod.Metadata.DisplayName)
                : Text("ui.selfCheckIssues", mod.Metadata.DisplayName, validation.Issues.Count);
        }

        GUILayout.EndVertical();
    }

    private static void DrawLogsPane(int height)
    {
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(height));
        GUILayout.BeginVertical(panelStyle!, GUILayout.Width(Math.Max(360, Screen.width / 3)), GUILayout.Height(height));
        GUILayout.Label(Text("ui.logs"), panelHeaderStyle!);
        var logRoot = Path.Combine(MelonEnvironment.UserDataDirectory, "FlashingLightsModKit");
        GUILayout.Label(logRoot, mutedStyle!, GUILayout.Height(22));

        var logFiles = GetLogFiles(logRoot);
        if (selectedLogPath != null && !logFiles.Contains(selectedLogPath))
        {
            selectedLogPath = null;
            logViewerText = string.Empty;
        }

        logsScroll = GUILayout.BeginScrollView(logsScroll);
        foreach (var path in logFiles)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(34));
            GUILayout.Label(Path.GetFileName(path), labelStyle!, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(Text("ui.view"), string.Equals(path, selectedLogPath, StringComparison.OrdinalIgnoreCase) ? selectedButtonStyle! : buttonStyle!, GUILayout.Width(70), GUILayout.Height(28)))
            {
                LoadLogViewer(path);
            }
            if (GUILayout.Button(Text("ui.copyPath"), buttonStyle!, GUILayout.Width(96), GUILayout.Height(28)))
            {
                GUIUtility.systemCopyBuffer = path;
                status = ModKitStrings.Get("ui.logPathCopied");
            }
            GUILayout.EndHorizontal();
        }

        if (logFiles.Count == 0)
        {
            GUILayout.Label(Text("ui.noLogs"), mutedStyle!);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(Gutter);
        GUILayout.BeginVertical(panelStyle!, GUILayout.ExpandWidth(true), GUILayout.Height(height));
        GUILayout.Label(selectedLogPath == null ? Text("ui.logViewer") : Path.GetFileName(selectedLogPath), panelHeaderStyle!);
        if (selectedLogPath == null)
        {
            GUILayout.Label(Text("ui.selectLog"), mutedStyle!);
        }
        else
        {
            GUILayout.BeginHorizontal(GUILayout.Height(34));
            GUILayout.Label(selectedLogPath, mutedStyle!, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(Text("ui.refresh"), buttonStyle!, GUILayout.Width(90), GUILayout.Height(28)))
            {
                LoadLogViewer(selectedLogPath);
            }
            GUILayout.EndHorizontal();
            logViewerScroll = GUILayout.BeginScrollView(logViewerScroll);
            GUILayout.TextArea(logViewerText, textAreaStyle!, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private static void DrawAboutPane(int height, bool expandWidth)
    {
        var options = expandWidth
            ? new[] { GUILayout.ExpandWidth(true), GUILayout.Height(height) }
            : new[] { GUILayout.Width(MinimumDetailsWidth), GUILayout.Height(height) };
        GUILayout.BeginVertical(panelStyle!, options);
        GUILayout.Label(Text("ui.tab.about"), panelHeaderStyle!);
        aboutScroll = GUILayout.BeginScrollView(aboutScroll);
        DrawInfoRow(Text("ui.info.sdk"), SdkInfo.Version);
        DrawInfoRow(Text("ui.info.github"), "Babyhamsta/FL-ModKit");
        DrawInfoRow(Text("ui.info.managedMods"), ModKitRegistry.ManagedMods.Count.ToString());

        if (TryGetSelectedMod(out var mod) && mod != null)
        {
            GUILayout.Space(12);
            GUILayout.Label(Text("ui.selectedMod"), panelHeaderStyle!);
            DrawInfoRow(Text("ui.info.name"), mod.Metadata.DisplayName);
            DrawInfoRow(Text("ui.info.version"), mod.Metadata.Version);
            DrawInfoRow(Text("ui.info.author"), mod.Metadata.Author);
            DrawInfoRow(Text("ui.info.license"), ModKitUiAboutPolicy.FormatLicense(mod.Metadata.License));
            DrawInfoRow(Text("ui.info.minSdk"), ModKitUiAboutPolicy.FormatMinSdkVersion(mod.Metadata.MinSdkVersion));
            DrawInfoRow(Text("ui.info.dependencies"), ModKitUiAboutPolicy.FormatDependencies(mod.Metadata.Dependencies));
            var modValidation = ModKitRegistry.GetValidation(mod.Metadata.Id);
            DrawInfoRow(Text("ui.info.status"), ModKitUiAboutPolicy.FormatValidationStatus(modValidation));
            if (!modValidation.IsValid)
            {
                foreach (var issue in modValidation.Issues)
                {
                    GUILayout.Label("• " + issue, mutedStyle!);
                }
            }
            if (!string.IsNullOrWhiteSpace(mod.Metadata.GitHubUrl))
            {
                DrawInfoRow(Text("ui.info.github"), mod.Metadata.GitHubUrl!);
            }
            if (!string.IsNullOrWhiteSpace(mod.Metadata.Changelog))
            {
                GUILayout.Label(mod.Metadata.Changelog, mutedStyle!);
            }
        }

        GUILayout.Space(12);
        if (GUILayout.Button(Text("ui.copyGithubUrl"), buttonStyle!, GUILayout.Height(32)))
        {
            GUIUtility.systemCopyBuffer = ModKitModMetadata.DefaultGitHubUrl;
            status = ModKitStrings.Get("ui.githubUrlCopied");
        }

        if (TryGetSelectedMod(out var selected) && selected?.ConfigAdapter != null)
        {
            if (GUILayout.Button(Text("ui.copyConfigPath"), buttonStyle!, GUILayout.Height(32)))
            {
                GUIUtility.systemCopyBuffer = selected.ConfigPath;
                status = ModKitStrings.Get("ui.configPathCopied");
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private static void DrawFooter()
    {
        GUILayout.BeginHorizontal(footerStyle!, GUILayout.Height(ModKitUiLayoutPolicy.FooterHeight));
        GUILayout.Label(status, mutedStyle!);
        GUILayout.FlexibleSpace();
        GUILayout.Label(ModKitModMetadata.DefaultGitHubUrl, mutedStyle!);
        GUILayout.EndHorizontal();
    }

    private static void DrawInfoRow(string label, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, mutedStyle!, GUILayout.Width(90));
        GUILayout.Label(value, valueStyle!);
        GUILayout.EndHorizontal();
    }

    private static void ApplyModEnabled(IModKitManagedMod mod, bool enabled)
    {
        if (enabled && !ModKitMultiplayerGuard.CanEnableMods())
        {
            status = ModKitMultiplayerPolicy.ToggleBlockedMessage;
            return;
        }

        if (enabled && ModKitRegistry.IsBlockedByManifest(mod.Metadata.Id))
        {
            status = ModKitUiAboutPolicy.FormatToggleBlockedMessage(mod.Metadata.DisplayName);
            return;
        }

        mod.SetEnabled(enabled);
        status = Text("ui.statusToggled", mod.Metadata.DisplayName, enabled ? Text("ui.enabled") : Text("ui.disabled"));
    }

    internal static bool ShouldBlockEnabledConfigEdit(IModKitManagedMod mod, bool requestedEnabled)
    {
        return requestedEnabled && ModKitRegistry.IsBlockedByManifest(mod.Metadata.Id);
    }

    private static bool IsEnabledProperty(ModKitConfigProperty property)
    {
        return string.Equals(property.Name, "Enabled", StringComparison.OrdinalIgnoreCase);
    }

    private static bool DrawSwitch(bool value, int width, bool disabled = false)
    {
        var rect = GUILayoutUtility.GetRect(width, 24, GUILayout.Width(width), GUILayout.Height(24));
        var nextValue = !disabled && GUI.Button(rect, GUIContent.none, GUIStyle.none) ? !value : value;

        var background = nextValue
            ? new Color(0.18f, 0.48f, 0.38f, disabled ? 0.58f : 1f)
            : new Color(0.18f, 0.21f, 0.28f, disabled ? 0.58f : 1f);
        DrawSolidRect(rect, background);

        var knobSize = 18;
        var knobX = nextValue ? rect.xMax - knobSize - 3 : rect.x + 3;
        var knobRect = new Rect(knobX, rect.y + 3, knobSize, knobSize);
        var labelRect = nextValue
            ? new Rect(rect.x + 6, rect.y, rect.width - knobSize - 12, rect.height)
            : new Rect(rect.x + knobSize + 6, rect.y, rect.width - knobSize - 12, rect.height);

        GUI.Label(labelRect, nextValue ? Text("ui.switchOn") : Text("ui.switchOff"), switchLabelStyle!);
        DrawSolidRect(knobRect, disabled
            ? new Color(0.55f, 0.59f, 0.66f, 1f)
            : new Color(0.92f, 0.94f, 0.97f, 1f));
        return nextValue;
    }

    private static IReadOnlyList<string> GetLogFiles(string logRoot)
    {
        try
        {
            if (!Directory.Exists(logRoot))
            {
                return Array.Empty<string>();
            }

            return Directory
                .EnumerateFiles(logRoot, "*.log", SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .Take(50)
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static void LoadLogViewer(string path)
    {
        try
        {
            selectedLogPath = path;
            logViewerText = ModKitUiLogViewerPolicy.TakeLastLines(File.ReadLines(path), 200);
            logViewerScroll = Vector2.zero;
            status = Text("ui.logLoaded", Path.GetFileName(path));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logViewerText = string.Empty;
            status = Text("ui.logReadFailed", ex.Message);
        }
    }

    private static bool IsResetPending(string modId)
    {
        if (pendingResetModId == null)
        {
            return false;
        }

        if (DateTimeOffset.UtcNow > pendingResetUntil)
        {
            pendingResetModId = null;
            return false;
        }

        return pendingResetModId.Equals(modId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HandlePendingHotkeyRebind(Event currentEvent)
    {
        if (pendingRebindModId == null
            || pendingRebindName == null
            || currentEvent.type != EventType.KeyDown
            || currentEvent.keyCode == KeyCode.None)
        {
            return false;
        }

        if (currentEvent.keyCode == KeyCode.Escape)
        {
            pendingRebindModId = null;
            pendingRebindName = null;
            status = ModKitStrings.Get("ui.hotkeyCanceled");
            currentEvent.Use();
            return true;
        }

        if (!ModKitRegistry.TryGet(pendingRebindModId, out var mod) || mod == null)
        {
            status = ModKitStrings.Get("ui.hotkeyModMissing");
        }
        else
        {
            try
            {
                status = mod.TryRebindHotkey(pendingRebindName, currentEvent.keyCode)
                    ? Text("ui.hotkeyRebound", mod.Metadata.DisplayName, currentEvent.keyCode)
                    : Text("ui.hotkeyNoRebind", mod.Metadata.DisplayName);
            }
            catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
            {
                status = ex.Message;
            }
        }

        pendingRebindModId = null;
        pendingRebindName = null;
        currentEvent.Use();
        return true;
    }

    private static void SaveDrafts(IModKitManagedMod mod, IModKitConfigAdapter adapter)
    {
        var modId = mod.Metadata.Id;
        foreach (var property in adapter.GetProperties())
        {
            if (!property.CanWrite)
            {
                continue;
            }

            var key = DraftKey(modId, property.Name);
            if (!DraftValues.TryGetValue(key, out var draft))
            {
                continue;
            }

            if (string.Equals(draft, property.ValueText, StringComparison.Ordinal))
            {
                continue;
            }

            if (IsEnabledProperty(property)
                && bool.TryParse(draft, out var requestedEnabled)
                && requestedEnabled
                && !ModKitMultiplayerGuard.CanEnableMods())
            {
                status = ModKitMultiplayerPolicy.ToggleBlockedMessage;
                return;
            }

            if (IsEnabledProperty(property)
                && bool.TryParse(draft, out requestedEnabled)
                && ShouldBlockEnabledConfigEdit(mod, requestedEnabled))
            {
                status = ModKitUiAboutPolicy.FormatToggleBlockedMessage(mod.Metadata.DisplayName);
                return;
            }

            if (!adapter.TrySetProperty(property.Name, draft, out var error))
            {
                status = error;
                return;
            }
        }

        adapter.Save();
        ClearDraftsForMod(modId);
        status = ModKitStrings.Get("ui.configSaved");
    }

    private static bool TryGetSelectedMod(out IModKitManagedMod? mod)
    {
        EnsureSelection();
        if (selectedModId == null)
        {
            mod = null;
            return false;
        }

        return ModKitRegistry.TryGet(selectedModId, out mod);
    }

    private static void EnsureSelection()
    {
        var mods = ModKitRegistry.ManagedMods;
        if (mods.Count == 0)
        {
            selectedModId = null;
            return;
        }

        if (selectedModId != null && mods.Any(mod => mod.Metadata.Id.Equals(selectedModId, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        SelectMod(mods[0].Metadata.Id);
    }

    private static void SelectMod(string modId)
    {
        selectedModId = modId;
        configScroll = Vector2.zero;
        status = ModKitStrings.Get("ui.configPulled");
    }

    private static void ClearDraftsForMod(string modId)
    {
        var prefix = modId + ":";
        foreach (var key in DraftValues.Keys.Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            DraftValues.Remove(key);
        }
    }

    private static string DraftKey(string modId, string propertyName)
    {
        return $"{modId}:{propertyName}";
    }

    private static void CaptureCursor()
    {
        if (capturedCursorState != null)
        {
            return;
        }

        capturedCursorState = new CursorState(Cursor.visible, Cursor.lockState);
        pendingCursorRestoreState = null;
        pendingCursorRestoreFrames = 0;
        EnsureCursor();
    }

    private static void EnsureCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private static void RestoreCursor()
    {
        if (capturedCursorState == null)
        {
            return;
        }

        pendingCursorRestoreState = capturedCursorState;
        pendingCursorRestoreFrames = ModKitUiInputPolicy.DeferredCursorRestoreFrames;
        capturedCursorState = null;
        RestorePendingCursorState();
    }

    private static void RestorePendingCursorState()
    {
        if (pendingCursorRestoreState == null || pendingCursorRestoreFrames <= 0)
        {
            pendingCursorRestoreState = null;
            pendingCursorRestoreFrames = 0;
            return;
        }

        ApplyCursorState(pendingCursorRestoreState.Value);
        pendingCursorRestoreFrames--;
        if (pendingCursorRestoreFrames <= 0)
        {
            pendingCursorRestoreState = null;
        }
    }

    private static void ApplyCursorState(CursorState state)
    {
        Cursor.lockState = state.LockState;
        Cursor.visible = state.Visible;
    }

    private static void EnsureStyles()
    {
        if (overlayBackgroundTexture == null)
        {
            overlayBackgroundTexture = Texture(new Color(0.02f, 0.025f, 0.035f, 0.98f));
        }

        headerStyle ??= BoxStyle(new Color(0.08f, 0.1f, 0.14f, 0.98f), 16, FontStyle.Bold);
        panelStyle ??= BoxStyle(new Color(0.075f, 0.09f, 0.13f, 0.96f), 12, FontStyle.Normal);
        panelHeaderStyle ??= LabelStyle(13, FontStyle.Bold, Color.white);
        selectedButtonStyle ??= ButtonStyle(new Color(0.18f, 0.44f, 0.35f, 1f), Color.white);
        buttonStyle ??= ButtonStyle(new Color(0.13f, 0.16f, 0.23f, 1f), new Color(0.88f, 0.91f, 0.96f, 1f));
        modRowStyle ??= ModRowStyle(new Color(0.13f, 0.16f, 0.23f, 1f));
        selectedModRowStyle ??= ModRowStyle(new Color(0.18f, 0.44f, 0.35f, 1f));
        modRowButtonStyle ??= ModRowButtonStyle();
        labelStyle ??= LabelStyle(13, FontStyle.Bold, Color.white);
        propertyLabelStyle ??= LabelStyle(12, FontStyle.Bold, new Color(0.92f, 0.94f, 0.98f, 1f));
        mutedStyle ??= LabelStyle(11, FontStyle.Normal, new Color(0.68f, 0.72f, 0.79f, 1f));
        valueStyle ??= LabelStyle(11, FontStyle.Normal, new Color(0.82f, 0.86f, 0.93f, 1f));
        textFieldStyle ??= TextFieldStyle();
        textAreaStyle ??= TextAreaStyle();
        footerStyle ??= FooterStyle();
        typeBadgeStyle ??= TypeBadgeStyle();
        switchLabelStyle ??= SwitchLabelStyle();
    }

    private static GUIStyle BoxStyle(Color color, int fontSize, FontStyle fontStyle)
    {
        var style = new GUIStyle(GUI.skin.box)
        {
            normal = { background = Texture(color), textColor = Color.white },
            padding = new RectOffset(12, 12, 9, 9),
            fontSize = fontSize,
            fontStyle = fontStyle,
            alignment = TextAnchor.MiddleLeft
        };
        return style;
    }

    private static GUIStyle ButtonStyle(Color color, Color textColor)
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            normal = { background = Texture(color), textColor = textColor },
            hover = { background = Texture(new Color(color.r + 0.04f, color.g + 0.04f, color.b + 0.04f, color.a)), textColor = textColor },
            active = { background = Texture(new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, color.a)), textColor = textColor },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            wordWrap = true
        };
        return style;
    }

    private static GUIStyle ModRowStyle(Color color)
    {
        return new GUIStyle(GUI.skin.box)
        {
            normal = { background = Texture(color), textColor = Color.white },
            hover = { background = Texture(new Color(color.r + 0.035f, color.g + 0.035f, color.b + 0.035f, color.a)), textColor = Color.white },
            active = { background = Texture(new Color(color.r * 0.82f, color.g * 0.82f, color.b * 0.82f, color.a)), textColor = Color.white },
            padding = new RectOffset(8, 8, 5, 5),
            margin = new RectOffset(0, 0, 3, 7),
            alignment = TextAnchor.MiddleLeft
        };
    }

    private static GUIStyle ModRowButtonStyle()
    {
        return new GUIStyle(GUI.skin.button)
        {
            normal = { background = Texture(new Color(0f, 0f, 0f, 0f)), textColor = new Color(0.9f, 0.93f, 0.98f, 1f) },
            hover = { background = Texture(new Color(1f, 1f, 1f, 0.045f)), textColor = Color.white },
            active = { background = Texture(new Color(0f, 0f, 0f, 0.12f)), textColor = Color.white },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            wordWrap = true,
            padding = new RectOffset(6, 6, 3, 3)
        };
    }

    private static GUIStyle LabelStyle(int fontSize, FontStyle fontStyle, Color color)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = fontStyle,
            normal = { textColor = color },
            wordWrap = true
        };
    }

    private static GUIStyle TextFieldStyle()
    {
        return new GUIStyle(GUI.skin.textField)
        {
            fontSize = 12,
            normal = { background = Texture(new Color(0.045f, 0.055f, 0.075f, 1f)), textColor = Color.white },
            focused = { background = Texture(new Color(0.065f, 0.08f, 0.11f, 1f)), textColor = Color.white },
            hover = { background = Texture(new Color(0.055f, 0.067f, 0.092f, 1f)), textColor = Color.white },
            padding = new RectOffset(8, 8, 4, 4),
            alignment = TextAnchor.MiddleLeft
        };
    }

    private static GUIStyle TextAreaStyle()
    {
        return new GUIStyle(GUI.skin.textArea)
        {
            fontSize = 12,
            normal = { background = Texture(new Color(0.045f, 0.055f, 0.075f, 1f)), textColor = Color.white },
            focused = { background = Texture(new Color(0.065f, 0.08f, 0.11f, 1f)), textColor = Color.white },
            hover = { background = Texture(new Color(0.055f, 0.067f, 0.092f, 1f)), textColor = Color.white },
            padding = new RectOffset(8, 8, 6, 6),
            wordWrap = false
        };
    }

    private static GUIStyle FooterStyle()
    {
        var style = BoxStyle(new Color(0.06f, 0.075f, 0.105f, 0.98f), 11, FontStyle.Normal);
        style.padding = new RectOffset(10, 10, 6, 4);
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    }

    private static GUIStyle TypeBadgeStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            normal = { background = Texture(new Color(0.12f, 0.15f, 0.22f, 1f)), textColor = new Color(0.78f, 0.83f, 0.91f, 1f) },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10,
            padding = new RectOffset(6, 6, 3, 3)
        };
    }

    private static GUIStyle SwitchLabelStyle()
    {
        return new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = new Color(0.94f, 0.96f, 0.99f, 1f) },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 9,
            fontStyle = FontStyle.Bold
        };
    }

    private static Texture2D Texture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static void DrawRect(Rect rect)
    {
        GUI.DrawTexture(rect, overlayBackgroundTexture ?? Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: true);
    }

    private static void DrawSolidRect(Rect rect, Color color)
    {
        var previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private readonly struct CursorState
    {
        public CursorState(bool visible, CursorLockMode lockState)
        {
            Visible = visible;
            LockState = lockState;
        }

        public bool Visible { get; }
        public CursorLockMode LockState { get; }
    }
}
