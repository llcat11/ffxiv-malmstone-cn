using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace Malmstone.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private Plugin Plugin;
<<<<<<< HEAD
    private string[] ToastOptions = {"普通", "任务", "错误"};

    public ConfigWindow(Plugin Plugin) : base("PVP通行证计算器 设置")
=======
    private string[] ToastOptions = {"Normal", "Quest", "Error"};

    public ConfigWindow(Plugin Plugin) : base("Malmstone Config")
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
    {
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(540, 390),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Configuration = Plugin.Configuration;
        this.Plugin = Plugin;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
    }

    public override void Draw()
    {
<<<<<<< HEAD
        ImGui.BeginTabBar("设置");

        // PVP Match Tab
        if (ImGui.BeginTabItem("PVP 比赛"))
        {
            ImGui.Text("目标系列赛等级");
=======
        ImGui.BeginTabBar("Settings");
        
        // PVP Match Tab
        if (ImGui.BeginTabItem("PVP Match"))
        {
            ImGui.Text("Goal Series Level");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
            var savedTargetSeriesRank = Configuration.DefaultTargetRankProperty;
            if (ImGui.InputInt("##SavedTargetSeriesRank", ref savedTargetSeriesRank, 1))
            {
                if (savedTargetSeriesRank < 1) savedTargetSeriesRank = 1;
                if (savedTargetSeriesRank > 107397) savedTargetSeriesRank = 107397;
                Configuration.DefaultTargetRankProperty = savedTargetSeriesRank;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
<<<<<<< HEAD
                ImGui.Text("计算器在初始化时会自动填入这个数字" +
                           "\n同时也会影响下方的通知覆盖设置");
=======
                ImGui.Text("Calculator will auto-populate with this number after initializing" +
                           "\nAlso controls the notification override settings below");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }

            var skipProgressionToastAfterGoal = Configuration.SkipProgressionToastAfterGoal;
            if (ImGui.Checkbox("###SkipProgressionToastAfterGoal", ref skipProgressionToastAfterGoal))
            {
                Configuration.SkipProgressionToastAfterGoal = skipProgressionToastAfterGoal;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
<<<<<<< HEAD
                    "达到目标系列赛等级后自动停止显示经验进度通知" +
                    "\n会覆盖其他经验通知设置");
=======
                    "Automatically stops showing EXP progression notification after Goal Series Level is reached" +
                    "\nOverrides other EXP notification settings");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
<<<<<<< HEAD
            ImGui.Text("达到目标系列赛等级后停止显示经验进度通知");
=======
            ImGui.Text("Skip EXP progression notifications after Goal Series Level is achieved");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1



            var skipProgressionChatAfterGoal = Configuration.SkipProgressionChatAfterGoal;
            if (ImGui.Checkbox("###SkipProgressionChatAfterGoal", ref skipProgressionChatAfterGoal))
            {
                Configuration.SkipProgressionChatAfterGoal = skipProgressionChatAfterGoal;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
<<<<<<< HEAD
                    "达到目标系列赛等级后自动停止显示剩余场数聊天消息" +
                    "\n会覆盖其他赛后聊天通知设置");
=======
                    "Automatically stops showing matches remaining chat messages after reaching the Goal Series Level" +
                    "\nOverrides other post-match chat notification settings");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
<<<<<<< HEAD
            ImGui.Text("达到目标系列赛等级后停止显示剩余场数聊天通知");
=======
            ImGui.Text("Skip remaining matches chat notifications after Goal Series Level is achieved");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1


            ImGui.Separator();

            var showProgressionToastPostMatch = Configuration.ShowProgressionToastPostMatch;
            if (ImGui.Checkbox("##ShowProgressionToastPostMatch", ref showProgressionToastPostMatch))
            {
                Configuration.ShowProgressionToastPostMatch = showProgressionToastPostMatch;
                if (showProgressionToastPostMatch)
                    Plugin.PvPAddon.EnablePostMatchProgressionToast();
                else
                    Plugin.PvPAddon.DisablePostMatchProgressionToast();
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
<<<<<<< HEAD
                ImGui.Text("在所有 PVP 比赛结束后显示包含当前系列赛等级经验进度的通知");
=======
                ImGui.Text("Shows a notification with current series level EXP progression after ALL PVP matches");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
<<<<<<< HEAD
            ImGui.Text("在 PVP 比赛后显示经验进度");

            ImGui.Text("通知类型");
=======
            ImGui.Text("Show EXP progression after PVP matches");

            ImGui.Text("Notification Type");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
            int selectedPostMatchToastType = Configuration.PostmatchProgressionToastType;
            if (ImGui.Combo("##MatchOptions", ref selectedPostMatchToastType, ToastOptions, ToastOptions.Length))
            {
                switch (selectedPostMatchToastType)
                {
                    case 0:
<<<<<<< HEAD
                        Plugin.ToastGui.ShowNormal("[PVP通行证计算器] 已选择普通通知");
                        break;
                    case 1:
                        Plugin.ToastGui.ShowQuest("[PVP通行证计算器] 已选择任务通知");
                        break;
                    case 2:
                        Plugin.ToastGui.ShowError("[PVP通行证计算器] 已选择错误通知");
=======
                        Plugin.ToastGui.ShowNormal("[Malmstone Calculator] Normal Toast Selected");
                        break;
                    case 1:
                        Plugin.ToastGui.ShowQuest("[Malmstone Calculator] Quest Toast Selected");
                        break;
                    case 2:
                        Plugin.ToastGui.ShowError("[Malmstone Calculator] Error Toast Selected");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                        break;
                }

                Configuration.PostmatchProgressionToastType = selectedPostMatchToastType;
                Configuration.Save();
            }

            ImGui.Separator();
<<<<<<< HEAD
            ImGui.Text("在赛后聊天中显示距离下一等级还需要的场数");
=======
            ImGui.Text("Show matches until next level in chat post-game");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1


            var showCCMatchesRemainingPostGame = Configuration.ShowProgressionChatPostCC;
            if (ImGui.Checkbox("##ShowCCMatchesRemainingPostGame", ref showCCMatchesRemainingPostGame))
            {
                Configuration.ShowProgressionChatPostCC = showCCMatchesRemainingPostGame;
                if (showCCMatchesRemainingPostGame)
                    Plugin.PvPAddon.EnableCrystallineConflictPostMatch();
                else
                    Plugin.PvPAddon.DisableCrystallineConflictPostMatch();
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
<<<<<<< HEAD
                    "在水晶冲突比赛结束后，于聊天中显示距离下一系列赛等级还需要的胜/负场数");
=======
                    "Show Wins/Losses needed until next Series Level in chat after Crystalline Conflict matches");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
<<<<<<< HEAD
            ImGui.Text("水晶冲突");
=======
            ImGui.Text("Crystalline Conflict");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1


            var showFLMatchesRemainingPostGame = Configuration.ShowProgressionChatPostFL;
            if (ImGui.Checkbox("##ShowFLMatchesRemainingPostGame", ref showFLMatchesRemainingPostGame))
            {
                Configuration.ShowProgressionChatPostFL = showFLMatchesRemainingPostGame;
                if (showFLMatchesRemainingPostGame && !Plugin.PvPAddon.FrontlineRecordPostSetupEnabled)
                    Plugin.PvPAddon.EnableFrontlinePostMatch();
                else if (!showFLMatchesRemainingPostGame && !Configuration.TrackFrontlineBonus)
                    Plugin.PvPAddon.DisableFrontlinePostMatch();
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
<<<<<<< HEAD
                    "在纷争前线比赛结束后，于聊天中显示距离下一系列赛等级还需要的名次次数\n括号内为每日挑战的场数");
=======
                    "Show placements needed until next Series Level in chat after Frontline matches\nRoulettes shown in parentheses");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
<<<<<<< HEAD
            ImGui.Text("纷争前线");
=======
            ImGui.Text("Frontlines");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1

            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();

            var trackFrontlineBonus = Configuration.TrackFrontlineBonus;
            if (ImGui.Checkbox("##TrackFrontlineBonus", ref trackFrontlineBonus))
            {
                Configuration.TrackFrontlineBonus = trackFrontlineBonus;
                if (trackFrontlineBonus && !Plugin.PvPAddon.FrontlineRecordPostSetupEnabled)
                    Plugin.PvPAddon.EnableFrontlinePostMatch();
                else if (!trackFrontlineBonus && !Configuration.ShowProgressionChatPostFL)
                    Plugin.PvPAddon.DisableFrontlinePostMatch();
                Configuration.OutdatedFrontlineRewardBonus = true;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
<<<<<<< HEAD
                ImGui.Text("（实验性）追踪纷争前线中连续获得第 3 名后获得的报酬补正" +
                           "\n第 3 名 = +10% 报酬补正（最高 50%）" +
                           "\n第 2 名 = 保持当前补正" +
                           "\n第 1 名 = 补正重置为 0\n");
=======
                ImGui.Text("(EXPERIMENTAL) Track the reward bonus you get for consecutive losses in Frontline" +
                           "\n3rd place = +10 percent bonus (max 50 percent)" +
                           "\n2nd place = Current bonus is kept" +
                           "\n1st Place = Bonus reset to 0\n");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
<<<<<<< HEAD
            ImGui.Text("追踪纷争前线报酬补正");
=======
            ImGui.Text("Track Frontline Reward Bonus");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1


            var showRWMatchesRemainingPostGame = Configuration.ShowProgressionChatPostRW;
            if (ImGui.Checkbox("##ShowRWMatchesRemainingPostGame", ref showRWMatchesRemainingPostGame))
            {
                Configuration.ShowProgressionChatPostRW = showRWMatchesRemainingPostGame;
                if (showRWMatchesRemainingPostGame)
                    Plugin.PvPAddon.EnableRivalWingsPostMatch();
                else
                    Plugin.PvPAddon.DisableRivalWingsPostMatch();
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
<<<<<<< HEAD
                ImGui.Text("在烈羽争锋比赛结束后，于聊天中显示距离下一系列赛等级还需要的胜/负场数");
=======
                ImGui.Text("Show Wins/Losses needed until next Series Level in chat after Rival Wings matches");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
<<<<<<< HEAD
            ImGui.Text("烈羽争锋");
=======
            ImGui.Text("Rival Wings");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1

            var OverrideShowMatchesToDefaultTargetGoal = Configuration.OverrideShowMatchesToDefaultTargetGoal;
            if (ImGui.Checkbox("##OverrideShowMatchesToDefaultTargetGoal", ref OverrideShowMatchesToDefaultTargetGoal))
            {
                Configuration.OverrideShowMatchesToDefaultTargetGoal = OverrideShowMatchesToDefaultTargetGoal;
                Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
<<<<<<< HEAD
                    "赛后聊天通知显示距离默认目标系列赛等级的剩余场数，而不是距离下一等级的场数" +
                    "\n仅当默认目标系列赛等级高于当前等级时生效，否则忽略此设置");
=======
                    "Show remaining matches to the Default Target rank instead of the next rank for postmatch chat notifications" +
                    "\nThis only works if the Default Target rank is higher than your current rank, otherwise this setting will be ignored");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
<<<<<<< HEAD
            ImGui.Text("显示距离目标等级的场数，而不是下一等级");
=======
            ImGui.Text("Show matches until Goal Series Level instead of next rank");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1


            ImGui.EndTabItem();
        }


        // User Interface Tab
<<<<<<< HEAD
        if (ImGui.BeginTabItem("界面"))
=======
        if (ImGui.BeginTabItem("User Interface"))
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
        {
            var showMainWindowOnPVPReward = Configuration.ShowMainWindowOnPVPReward;
            if (ImGui.Checkbox("##ShowMainWindowOnPVPReward", ref showMainWindowOnPVPReward))
            {
                Configuration.ShowMainWindowOnPVPReward = showMainWindowOnPVPReward;
                Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
<<<<<<< HEAD
                ImGui.Text("查看系列PVP通行证奖励时自动打开计算器窗口");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("查看 PVP 系列奖励时显示主窗口");

            ImGui.Separator();

            ImGui.Text("在以下位置显示真实系列等级：");
=======
                ImGui.Text("Automatically open the calculator window when viewing Series Malmstone rewards");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("Show main window when viewing PVP Series Rewards");
            
            ImGui.Separator();

            ImGui.Text("Display TRUE Series Level in...");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
<<<<<<< HEAD
                    "FFXIV 在系列等级超过 30 后就不再继续追踪。\nPVP通行证计算器可以追踪并显示远高于上限的实际系列等级");
=======
                    "FFXIV stops tracking Series Level after 30.\nMalmstone Calculator can track and display your actual series level far beyond the maximum");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
                ImGui.EndTooltip();
            }
            var showTrueSeriesLevelPVPReward = Configuration.ShowTrueSeriesLevelPVPReward;
            if (ImGui.Checkbox("##ShowTrueSeriesLevelPVPReward", ref showTrueSeriesLevelPVPReward))
            {
                if (showTrueSeriesLevelPVPReward)
                    Plugin.EnablePVPRewardReplaceSeriesLevel();
                else
                    Plugin.DisablePVPRewardReplaceSeriesLevel();
                Configuration.ShowTrueSeriesLevelPVPReward = showTrueSeriesLevelPVPReward;
                Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
<<<<<<< HEAD
                ImGui.Text("在 PVP 奖励窗口中显示插件追踪的系列等级（支持 30 级以上）" +
                           "\n[会修改游戏的原生 UI]");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("PVP 奖励窗口");

=======
                ImGui.Text("Show plugin tracked Series Level in the PVP Reward window (supports Series Levels above 30)" +
                           "\n[This modifies the game's native UI]");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("PVP Reward window");
            
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
            var showTrueSeriesLevelPVPProfile = Configuration.ShowTrueSeriesLevelPVPProfile;
            if (ImGui.Checkbox("##ShowTrueSeriesLevelPVPProfile", ref showTrueSeriesLevelPVPProfile))
            {
                if (showTrueSeriesLevelPVPProfile)
                    Plugin.EnablePVPProfileReplaceSeriesLevel();
                else
                    Plugin.DisablePVPProfileReplaceSeriesLevel();
                Configuration.ShowTrueSeriesLevelPVPProfile = showTrueSeriesLevelPVPProfile;
                Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
<<<<<<< HEAD
                ImGui.Text("在 PVP 档案窗口中显示插件追踪的系列等级（支持 30 级以上）" +
                           "\n[会修改游戏的原生 UI]");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("PVP 档案窗口");


=======
                ImGui.Text("Show plugin tracked Series Level in the PVP Profile window (supports Series Levels above 30)" +
                           "\n[This modifies the game's native UI]");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("PVP Profile window");
            
            
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
        ImGui.Separator();
<<<<<<< HEAD
        ImGui.Text("修改已自动保存");
=======
        ImGui.Text("Changes saved automatically");
>>>>>>> dbffb5c6f99683e459e9a36432a6221c5c4f71f1

    }
}
