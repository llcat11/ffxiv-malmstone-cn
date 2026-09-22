using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace Malmstone.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private Plugin Plugin;
    private string[] ToastOptions = {"普通", "任务", "错误"};

    public ConfigWindow(Plugin Plugin) : base("PVP通行证计算器 设置")
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
        ImGui.BeginTabBar("设置");

        // PVP Match Tab
        if (ImGui.BeginTabItem("PVP 比赛"))
        {
            ImGui.Text("目标系列赛等级");
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
                ImGui.Text("计算器在初始化时会自动填入这个数字" +
                           "\n同时也会影响下方的通知覆盖设置");
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
                    "达到目标系列赛等级后自动停止显示经验进度通知" +
                    "\n会覆盖其他经验通知设置");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("达到目标系列赛等级后停止显示经验进度通知");



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
                    "达到目标系列赛等级后自动停止显示剩余场数聊天消息" +
                    "\n会覆盖其他赛后聊天通知设置");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("达到目标系列赛等级后停止显示剩余场数聊天通知");


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
                ImGui.Text("在所有 PVP 比赛结束后显示包含当前系列赛等级经验进度的通知");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("在 PVP 比赛后显示经验进度");

            ImGui.Text("通知类型");
            int selectedPostMatchToastType = Configuration.PostmatchProgressionToastType;
            if (ImGui.Combo("##MatchOptions", ref selectedPostMatchToastType, ToastOptions, ToastOptions.Length))
            {
                switch (selectedPostMatchToastType)
                {
                    case 0:
                        Plugin.ToastGui.ShowNormal("[PVP通行证计算器] 已选择普通通知");
                        break;
                    case 1:
                        Plugin.ToastGui.ShowQuest("[PVP通行证计算器] 已选择任务通知");
                        break;
                    case 2:
                        Plugin.ToastGui.ShowError("[PVP通行证计算器] 已选择错误通知");
                        break;
                }

                Configuration.PostmatchProgressionToastType = selectedPostMatchToastType;
                Configuration.Save();
            }

            ImGui.Separator();
            ImGui.Text("在赛后聊天中显示距离下一等级还需要的场数");


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
                    "在水晶冲突比赛结束后，于聊天中显示距离下一系列赛等级还需要的胜/负场数");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("水晶冲突");


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
                    "在纷争前线比赛结束后，于聊天中显示距离下一系列赛等级还需要的名次次数\n括号内为每日挑战的场数");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("纷争前线");

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
                ImGui.Text("（实验性）追踪纷争前线中连续获得第 3 名后获得的报酬补正" +
                           "\n第 3 名 = +10% 报酬补正（最高 50%）" +
                           "\n第 2 名 = 保持当前补正" +
                           "\n第 1 名 = 补正重置为 0\n");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("追踪纷争前线报酬补正");


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
                ImGui.Text("在烈羽争锋比赛结束后，于聊天中显示距离下一系列赛等级还需要的胜/负场数");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("烈羽争锋");

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
                    "赛后聊天通知显示距离默认目标系列赛等级的剩余场数，而不是距离下一等级的场数" +
                    "\n仅当默认目标系列赛等级高于当前等级时生效，否则忽略此设置");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.Text("显示距离目标等级的场数，而不是下一等级");


            ImGui.EndTabItem();
        }


        // User Interface Tab
        if (ImGui.BeginTabItem("界面"))
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
                ImGui.Text("查看系列PVP通行证奖励时自动打开计算器窗口");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("查看 PVP 系列奖励时显示主窗口");

            ImGui.Separator();

            ImGui.Text("在以下位置显示真实系列等级：");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(
                    "FFXIV 在系列等级超过 30 后就不再继续追踪。\nPVP通行证计算器可以追踪并显示远高于上限的实际系列等级");
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
                ImGui.Text("在 PVP 奖励窗口中显示插件追踪的系列等级（支持 30 级以上）" +
                           "\n[会修改游戏的原生 UI]");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("PVP 奖励窗口");

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
                ImGui.Text("在 PVP 档案窗口中显示插件追踪的系列等级（支持 30 级以上）" +
                           "\n[会修改游戏的原生 UI]");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.Text("PVP 档案窗口");


            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
        ImGui.Separator();
        ImGui.Text("修改已自动保存");

    }
}
