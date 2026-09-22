using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Malmstone.Windows;
using Malmstone.Services;
using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using System.Linq;
using Malmstone.Utils;
using Malmstone.Addons;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

namespace Malmstone;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui Chat { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifeCycle { get; private set; } = null!;
    [PluginService] internal static IToastGui ToastGui { get; private set; } = null!;
    [PluginService] internal static IPluginLog Logger { get; set; } = default!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    private const string CommandName = "/pmalm";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Malmstone");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    internal readonly PvPService PvPService;
    internal PvPMatchAddon PvPAddon;
    internal UIChanger UIChanger;
    internal int CachedSeriesLevel;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        PvPService = new PvPService();
        PvPAddon = new PvPMatchAddon(this);
        UIChanger = new UIChanger(this);
        if (Configuration.ShowProgressionChatPostCC)
            PvPAddon.EnableCrystallineConflictPostMatch();
        if (Configuration.ShowProgressionChatPostRW)
            PvPAddon.EnableRivalWingsPostMatch();
        if (Configuration.ShowProgressionChatPostFL || Configuration.TrackFrontlineBonus)
            PvPAddon.EnableFrontlinePostMatch();
        if (Configuration.ShowProgressionToastPostMatch)
            PvPAddon.EnablePostMatchProgressionToast();
        if(Configuration.ShowTrueSeriesLevelPVPProfile)
            EnablePVPProfileReplaceSeriesLevel();
        if(Configuration.ShowTrueSeriesLevelPVPReward)
            EnablePVPRewardReplaceSeriesLevel();
            
        EnablePVPRewardWindowAddon();

        if (Configuration.IsPrimedForBuff)
            PvPService.ConsecutiveThirdPlaceFrontline = 1;

        if (Configuration.PostmatchProgressionToastType < 0 || Configuration.PostmatchProgressionToastType > 2)
        {
            Configuration.PostmatchProgressionToastType = 0;
        }

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "/pmalm <等级> <all/cc/fl/rw> -- 显示距离目标系列赛等级还需要的场数。cc = 水晶冲突，fl = 纷争前线，rw = 烈羽争锋"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        Framework.Update += CheckPlayerLoaded;
        ClientState.Login += OnLogin;
        ClientState.Logout += OnLogout;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        if (Configuration.ShowProgressionChatPostCC)
            PvPAddon.DisableCrystallineConflictPostMatch();
        if (Configuration.ShowProgressionChatPostRW)
            PvPAddon.DisableRivalWingsPostMatch();
        if (Configuration.ShowProgressionChatPostFL || Configuration.TrackFrontlineBonus)
            PvPAddon.DisableFrontlinePostMatch();
        if (Configuration.ShowProgressionToastPostMatch)
            PvPAddon.DisablePostMatchProgressionToast();
        if (Configuration.ShowTrueSeriesLevelPVPProfile)
            DisablePVPProfileReplaceSeriesLevel();
        if (Configuration.ShowTrueSeriesLevelPVPReward)
            DisablePVPRewardReplaceSeriesLevel();
        DisablePVPRewardWindowAddon();
        
        CommandManager.RemoveHandler(CommandName);
    }

private void OnCommand(string command, string args)
{
    if (string.IsNullOrWhiteSpace(args))
    {
        ToggleMainUI();
        return;
    }

    var splitArgs = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var specs = new HashSet<string>(splitArgs.Skip(1).Select(spec => spec.ToLower()));

    var pvpInfo = PvPService.GetPvPSeriesInfo();

    if (pvpInfo == null) return;
    var CurrentSeriesLevel = pvpInfo.CurrentSeriesRank + GetSavedExtraLevels(); 
    if (!int.TryParse(splitArgs[0], out int targetRank))
    {
        if (splitArgs[0] == "next")
        {
            targetRank = CurrentSeriesLevel + 1;
        }
        else if (splitArgs[0] == "config")
        {
            ToggleConfigUI();
            return;
        }
        else return;

    }
    // Show games left in chat log when there are args

    if (targetRank < 1)
    {
        Chat.PrintError("目标系列赛等级不能小于 1");
        return;
    }

    if (targetRank > 107397)
    {
        Chat.PrintError("目标系列赛等级不能大于 107397（你真的觉得自己能达到吗？）");
        return;
    }

    if (targetRank < CurrentSeriesLevel)
    {
        Chat.PrintError("你已经超过系列赛等级 " + targetRank + " 了");
        return;
    }

    var xpResult = MalmstoneXPCalculator.CalculateXp(
        CurrentSeriesLevel,
        targetRank,
        pvpInfo.SeriesExperience);

    bool includeAll = specs.Contains("all");
    if (!specs.Any())
    {
        includeAll = true;
    }
    var seString = new SeString(new List<Payload>());
    seString.Append(new TextPayload("\n[距离系列赛等级 " + targetRank + "]"));

    // Crystalline Conflict
    if (includeAll || specs.Contains("cc"))
    {
        seString.Append(new TextPayload("\n水晶冲突：\n"));
        seString.Append(new UIForegroundPayload(35));

        if (xpResult.CrystallineConflictWin > 0)
        {
            seString.Append(new TextPayload($"胜利：{xpResult.CrystallineConflictWin} " + (xpResult.CrystallineConflictWin == 1 ? "场" : "场") + "\n"));
        }

        if (xpResult.CrystallineConflictLose > 0)
        {
            seString.Append(new TextPayload($"失败：{xpResult.CrystallineConflictLose} " + (xpResult.CrystallineConflictLose == 1 ? "场" : "场") + "\n"));
        }

        if(xpResult.CrystallineConflictExpectedMatches > 0)
        {
            seString.Append(new TextPayload($"预计：{xpResult.CrystallineConflictExpectedMatches} " + (xpResult.CrystallineConflictExpectedMatches == 1 ? "场" : "场") + "\n"));
        }

        seString.Append(UIForegroundPayload.UIForegroundOff);
    }

    //Frontlines
    if (includeAll || specs.Contains("fl"))
    {
        seString.Append(new TextPayload("\n纷争前线：\n"));
        seString.Append(new UIForegroundPayload(518));

        if (xpResult.FrontlineWin > 0)
        {
            seString.Append(new TextPayload($"第 1 名：{xpResult.FrontlineWin} " + (xpResult.FrontlineWin == 1 ? "场" : "场") + "\n"));
        }

        if (xpResult.FrontlineLose2nd > 0)
        {
            seString.Append(new TextPayload($"第 2 名：{xpResult.FrontlineLose2nd} " + (xpResult.FrontlineLose2nd == 1 ? "场" : "场") + "\n"));
        }

        if (xpResult.FrontlineLose3rd > 0)
        {
            seString.Append(new TextPayload($"第 3 名：{xpResult.FrontlineLose3rd} " + (xpResult.FrontlineLose3rd == 1 ? "场" : "场") + "\n"));
        }

        if (xpResult.FrontlineExpectedMatches > 0)
        {
            seString.Append(new TextPayload($"预计：{xpResult.FrontlineExpectedMatches} " + (xpResult.FrontlineExpectedMatches == 1 ? "场" : "场") + "\n"));
        }

        seString.Append(UIForegroundPayload.UIForegroundOff);

        seString.Append(new TextPayload("\n纷争前线（每日挑战）：\n"));
        seString.Append(new UIForegroundPayload(518));

        if (xpResult.FrontlineDailyWin > 0)
        {
            seString.Append(new TextPayload($"第 1 名：{xpResult.FrontlineDailyWin} " + (xpResult.FrontlineDailyWin == 1 ? "场" : "场") + "\n"));
        }

        if (xpResult.FrontlineDailyLose2nd > 0)
        {
            seString.Append(new TextPayload($"第 2 名：{xpResult.FrontlineDailyLose2nd} " + (xpResult.FrontlineDailyLose2nd == 1 ? "场" : "场") + "\n"));
        }

        if (xpResult.FrontlineDailyLose3rd > 0)
        {
            seString.Append(new TextPayload($"第 3 名：{xpResult.FrontlineDailyLose3rd} " + (xpResult.FrontlineDailyLose3rd == 1 ? "场" : "场") + "\n"));
        }

        if (xpResult.FrontlineDailyExpectedMatches > 0)
        {
            seString.Append(new TextPayload($"预计：{xpResult.FrontlineDailyExpectedMatches} " + (xpResult.FrontlineDailyExpectedMatches == 1 ? "场" : "场") + "\n"));
        }

        seString.Append(UIForegroundPayload.UIForegroundOff);
    }

    // Rival Wings
    if (includeAll || specs.Contains("rw"))
    {
        seString.Append(new TextPayload("\n烈羽争锋：\n"));
        seString.Append(new UIForegroundPayload(43));

        if (xpResult.RivalWingsWin > 0)
        {
            seString.Append(new TextPayload($"胜利：{xpResult.RivalWingsWin} " + (xpResult.RivalWingsWin == 1 ? "场" : "场") + "\n"));
        }

        if (xpResult.RivalWingsLose > 0)
        {
            seString.Append(new TextPayload($"失败：{xpResult.RivalWingsLose} " + (xpResult.RivalWingsLose == 1 ? "场" : "场") + "\n"));
        }

        if (xpResult.RivalWingsExpectedMatches > 0)
        {
            seString.Append(new TextPayload($"预计：{xpResult.RivalWingsExpectedMatches} " + (xpResult.RivalWingsExpectedMatches == 1 ? "场" : "场") + "\n"));
        }

        seString.Append(UIForegroundPayload.UIForegroundOff);
    }

    if (seString.Payloads.Count > 0) Chat.Print(seString);
}


    private void CheckPlayerLoaded(IFramework framework)
    {
        if(PlayerState.IsLoaded)
        {
            ulong contentId = PlayerState.ContentId;
            int CurrentSeriesLevel = PvPService.GetPvPSeriesInfo()?.CurrentSeriesRank ?? 0;
            if (Configuration.ExtraLevelsMap.TryGetValue(contentId, out var extraLevels))
            {
                if(extraLevels >= 1 && CurrentSeriesLevel < 30)
                {
                    Logger.Debug("Extra Levels has expired likely due to a new Series Malmstone. Resetting to 0");
                    Configuration.ExtraLevelsMap[contentId] = 0;
                    Configuration.Save();
                }
            }


            if (Configuration.TrackFrontlineBonus)
            {
                Logger.Debug("Player has loaded in. Attempting to get Frontline PVP Profile Data");
                PvPService.UpdateFrontlineResultCache();
                Logger.Debug("Initial Frontline Data Cached As: First: " + PvPService.CachedFrontlineResults.FirstPlace + 
                    " Second: " + PvPService.CachedFrontlineResults.SecondPlace + " Third: " + PvPService.CachedFrontlineResults.ThirdPlace);
                Framework.Update -= CheckPlayerLoaded;
            }
        }
    }

    private void OnLogin()
    {
        Logger.Debug("Player has logged in. Waiting for player data to load...");
        Framework.Update += CheckPlayerLoaded;
    }

    private void OnLogout(int _, int __) => Framework.Update -= CheckPlayerLoaded;
    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    public void OnOpenPVPRewardWindow(AddonEvent eventType, AddonArgs addonInfo)
    {
        PvPSeriesInfo? PvPSeriesInfo = PvPService.GetPvPSeriesInfo();
        if(PvPSeriesInfo != null)
            CachedSeriesLevel = PvPSeriesInfo.CurrentSeriesRank;
        Logger.Debug("PVPRewardWindow Open, Current Series Level Cached: " + CachedSeriesLevel);
        MainWindow.OnOpenPVPRewardWindow();
    }

    public void OnClosePVPRewardWindow(AddonEvent eventType, AddonArgs addonInfo) =>
        MainWindow.OnClosePVPRewardWindow();

    public void UpdateExtraLevels(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (PvPService.GetPvPSeriesInfo() != null)
        {
            // If player claimed Extra Level reward (above Level 30) and we detect a decrease in Series level
            PvPSeriesInfo? PvPSeriesInfo = PvPService.GetPvPSeriesInfo();
            if (PvPSeriesInfo == null)
                return;
            if(PvPSeriesInfo.CurrentSeriesRank < CachedSeriesLevel && CachedSeriesLevel > 30)   
            {
                Logger.Debug("Player claimed extra levels: Old Level is " + GetSavedExtraLevels());
                if (IncrementExtraLevels(1))
                    Logger.Debug("Successfully incremented extra levels for player");
                else
                    Logger.Debug("Failed to increment extra levels for player");
                Configuration.Save();
                CachedSeriesLevel = PvPSeriesInfo.CurrentSeriesRank;
            }
            else
            {
                Logger.Debug("Player did not claim any extra ranks");
            }
        }
    }
    
    public int GetSavedExtraLevels()
    {
        ulong contentId = PlayerState.ContentId;
        if (Configuration.ExtraLevelsMap.TryGetValue(contentId, out var extraLevels))
        {
            return extraLevels;
        }
        Logger.Debug("No Extra Levels saved for this character");
        int CurrentSeriesLevel = PvPService.GetPvPSeriesInfo()?.CurrentSeriesRank ?? 0;
        if (CurrentSeriesLevel > 30)
        {
            Configuration.ExtraLevelsMap[contentId] = CurrentSeriesLevel - 30;
            Configuration.Save();
            Logger.Debug("Extra Levels saved for this character: " + (CurrentSeriesLevel - 30));
            return CurrentSeriesLevel - 30;
        }
        Logger.Debug("Extra Levels saved for this character: 0");
        Configuration.ExtraLevelsMap[contentId] = 0;
        Configuration.Save();
        return 0;
    }

    public bool IncrementExtraLevels(int amount)
    {
        ulong contentId = PlayerState.ContentId;
        if (Configuration.ExtraLevelsMap.TryGetValue(contentId, out var extraLevels))
        {
            Configuration.ExtraLevelsMap[contentId] = extraLevels + amount;
            Configuration.Save();
            Logger.Debug("Extra Levels incremented for this character: " + (extraLevels + amount));
            return true;
        }
        Logger.Debug("Failed to increment extra levels for this character");
        return false;
    }

    public void EnablePVPRewardWindowAddon()
    {
        AddonLifeCycle.RegisterListener(AddonEvent.PostSetup, "PvpReward", OnOpenPVPRewardWindow);
        AddonLifeCycle.RegisterListener(AddonEvent.PostRefresh, "PvpReward", UpdateExtraLevels);
        AddonLifeCycle.RegisterListener(AddonEvent.PreFinalize, "PvpReward", OnClosePVPRewardWindow);
    }
    public void DisablePVPRewardWindowAddon()
    {
        AddonLifeCycle.UnregisterListener(AddonEvent.PostSetup, "PvpReward", OnOpenPVPRewardWindow);
        AddonLifeCycle.UnregisterListener(AddonEvent.PostRefresh, "PvpReward", UpdateExtraLevels);
        AddonLifeCycle.UnregisterListener(AddonEvent.PreFinalize, "PvpReward", OnClosePVPRewardWindow);
    }
    
    public void EnablePVPProfileReplaceSeriesLevel() => AddonLifeCycle.RegisterListener(AddonEvent.PostDraw, "PvpProfile", UIChanger.ReplacePVPProfileWindowSeriesRank);
    public void DisablePVPProfileReplaceSeriesLevel() => AddonLifeCycle.UnregisterListener(AddonEvent.PostDraw, "PvpProfile", UIChanger.ReplacePVPProfileWindowSeriesRank);
    public void EnablePVPRewardReplaceSeriesLevel() => AddonLifeCycle.RegisterListener(AddonEvent.PostDraw, "PvpReward", UIChanger.ReplacePVPRewardWindowSeriesRank);
    public void DisablePVPRewardReplaceSeriesLevel() => AddonLifeCycle.UnregisterListener(AddonEvent.PostDraw, "PvpReward", UIChanger.ReplacePVPRewardWindowSeriesRank);

}

