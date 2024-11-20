using Dalamud.Interface.Utility.Raii;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using LazyLeves.Config;

namespace LazyLeves.Windows;

public class ConfigWindow : SimpleWindow
{
    private readonly TextService TextService;
    private readonly PluginConfig PluginConfig;

    public ConfigWindow(
        WindowManager windowManager,
        TextService textService,
        PluginConfig pluginConfig)
        : base(windowManager, textService.Translate("WindowTitle.Configuration"))
    {
        TextService = textService;
        PluginConfig = pluginConfig;

        AllowClickthrough = false;
        AllowPinning = false;
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void Draw()
    {
        using var tabbar = ImRaii.TabBar("LeveHelperTabs", ImGuiTabBarFlags.Reorderable);
        if (!tabbar) return;
    }
}
