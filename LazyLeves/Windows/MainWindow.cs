using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.File;
using HaselCommon.Extensions.Strings;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace LazyLeves.Windows;

public class MainWindow : SimpleWindow
{
    private readonly TextService TextService;
    private readonly AddonObserver AddonObserver;
    private readonly IDalamudPluginInterface PluginInterface;
    private readonly IClientState ClientState;
    private readonly ICommandManager CommandManager;
    private readonly IGameInventory GameInventory;
    private readonly ConfigWindow ConfigWindow;
    private readonly LeveService LeveService;
    private readonly FiltersState FiltersState;
    private readonly ExcelService ExcelService;

    private readonly CommandInfo CommandInfo;

    private IEnumerable<ushort> LastActiveLevequestIds = [];
    private int listIndex = 0;

    public MainWindow(
        WindowManager windowManager,
        TextService textService,
        AddonObserver addonObserver,
        IDalamudPluginInterface pluginInterface,
        IClientState clientState,
        ICommandManager commandManager,
        IGameInventory gameInventory,
        ConfigWindow configWindow,
        LeveService leveService,
        FiltersState filtersState,
        ExcelService excelService) : base(windowManager, textService.Translate("WindowTitle.Main"))
    {
        TextService = textService;
        AddonObserver = addonObserver;
        PluginInterface = pluginInterface;
        ClientState = clientState;
        CommandManager = commandManager;
        GameInventory = gameInventory;
        ConfigWindow = configWindow;
        LeveService = leveService;
        FiltersState = filtersState;
        ExcelService = excelService;

        Size = new Vector2(830, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(400, 400),
            MaximumSize = new Vector2(4096, 2160)
        };

        TitleBarButtons.Add(new()
        {
            Icon = Dalamud.Interface.FontAwesomeIcon.Cog,
            IconOffset = new(0, 1),
            ShowTooltip = () => ImGui.SetTooltip(textService.Translate($"TitleBarButton.ToggleConfig.Tooltip.{(ConfigWindow.IsOpen ? "Close" : "Open")}Config")),
            Click = (button) => { ConfigWindow.Toggle(); }
        });

#if DEBUG
        DebugTab = debugTab;
#endif

        CommandInfo = new CommandInfo((_, _) => Toggle())
        {
            HelpMessage = textService.Translate("LeveHelper.CommandHandlerHelpMessage")
        };

        CommandManager.AddHandler("/lazyleves", CommandInfo);
        CommandManager.AddHandler("/lz", CommandInfo);

        PluginInterface.UiBuilder.OpenMainUi += Toggle;
        PluginInterface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;
        TextService.LanguageChanged += OnLanguageChanged;
        AddonObserver.AddonOpen += OnAddonOpen;
        AddonObserver.AddonClose += OnAddonClose;
        GameInventory.InventoryChangedRaw += OnInventoryChangedRaw;
    }

    public new void Dispose()
    {
        GameInventory.InventoryChangedRaw -= OnInventoryChangedRaw;
        AddonObserver.AddonOpen -= OnAddonOpen;
        AddonObserver.AddonClose -= OnAddonClose;
        TextService.LanguageChanged -= OnLanguageChanged;
        PluginInterface.UiBuilder.OpenConfigUi -= ConfigWindow.Toggle;
        PluginInterface.UiBuilder.OpenMainUi -= Toggle;
        CommandManager.RemoveHandler("/lazyleves");
        CommandManager.RemoveHandler("/lz");
        base.Dispose();
    }

    private void OnLanguageChanged(string langCode)
    {
        CommandInfo.HelpMessage = TextService.Translate("LeveHelper.CommandHandlerHelpMessage");
    }

    private void Refresh()
    {
    }

    private void OnInventoryChangedRaw(IReadOnlyCollection<InventoryEventArgs> events)
    {
        Refresh();
    }

    private void OnAddonOpen(string addonName)
    {
        if (addonName is "Catch")
            Refresh();
    }

    private void OnAddonClose(string addonName)
    {
        if (addonName is "Synthesis" or "SynthesisSimple" or "Gathering" or "ItemSearchResult" or "InclusionShop" or "Shop" or "ShopExchangeCurrency" or "ShopExchangeItem")
            Refresh();
    }

    public override void Update()
    {
    }

    public override bool DrawConditions() => ClientState.IsLoggedIn;

    public override void Draw()
    {
        using var tabbar = ImRaii.TabBar("LeveHelperTabs", ImGuiTabBarFlags.Reorderable);
        if (!tabbar) return;

        FiltersState.Reload();
        var allLeves = FiltersState.AllLeves;
        var allLevesNames = new List<string>();
        foreach (var leve in allLeves)
        {
            if (LeveService.IsCraftLeve(leve))
            {
                allLevesNames.Add(leve.Name.ToDalamudString().ToString());
            }
        }
        var issuerDropdown = ImGui.ListBox("Size: " + allLevesNames.Count, ref listIndex, allLevesNames.ToArray(), allLevesNames.Count);
        var levesDropdown = ImGui.ListBox("Size: " + allLevesNames.Count, ref listIndex, allLevesNames.ToArray(), allLevesNames.Count);
    }
}
