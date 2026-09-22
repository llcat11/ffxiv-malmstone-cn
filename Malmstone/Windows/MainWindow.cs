using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Malmstone.Services;
using Malmstone.Utils;

namespace Malmstone.Windows
{
    public class MainWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private PvPService PvPService;
        private int TargetSeriesRank;

        // Cache-related fields
        private int _lastSeriesRank;
        private int _lastTargetSeriesRank;
        private int _lastSeriesExperience;
        private MalmstoneXPCalculator.XpCalculationResult _cachedXpResult;
        
        public bool IgnoreSeriesLevelUpdates { get; set; } = false;

        public MainWindow(Plugin plugin)
            : base("PVP通行证计算器")
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(460, 630),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            Plugin = plugin;
            PvPService = new PvPService();
            TargetSeriesRank = Plugin.Configuration.DefaultTargetRankProperty;
        }

        public void Dispose() { }

        public override void Draw()
        {
            if (!IsOpen) return;
            var pvpInfo = PvPService.GetPvPSeriesInfo();
            if (pvpInfo != null)
            {
                var CurrentSeriesLevel = pvpInfo.CurrentSeriesRank + Plugin.GetSavedExtraLevels();
                ImGui.Text($"当前系列赛等级：{CurrentSeriesLevel}");
                ImGui.Text($"当前等级经验进度：{pvpInfo.SeriesExperience}");
                ImGui.Spacing();

                ImGui.Text("目标系列赛等级：");
                ImGui.InputInt("##TargetSeriesRank", ref TargetSeriesRank, 1);

                // Bounds checking to ensure no overflows
                if (TargetSeriesRank < 1) TargetSeriesRank = 1;
                if (TargetSeriesRank > 107397) TargetSeriesRank = 107397;

                if (TargetSeriesRank <= CurrentSeriesLevel) TargetSeriesRank = CurrentSeriesLevel + 1;

                ImGui.Spacing();
                ImGui.Separator();

                // Only recalculate if the relevant data has changed
                if (pvpInfo.CurrentSeriesRank != _lastSeriesRank ||
                    TargetSeriesRank != _lastTargetSeriesRank ||
                    pvpInfo.SeriesExperience != _lastSeriesExperience)
                {
                    _cachedXpResult = MalmstoneXPCalculator.CalculateXp(CurrentSeriesLevel, TargetSeriesRank, pvpInfo.SeriesExperience);
                    _lastSeriesRank = pvpInfo.CurrentSeriesRank;
                    _lastTargetSeriesRank = TargetSeriesRank;
                    _lastSeriesExperience = pvpInfo.SeriesExperience;
                }

                var xpResult = _cachedXpResult;

                ImGui.Spacing();
                ImGui.Text($"距离等级 {xpResult.TargetLevel} 还差 {xpResult.RemainingXp} 系列赛经验");

                // If player has 100 unclaimed series levels (+30 for all main malmstones)
                if(pvpInfo.CurrentSeriesRank > 130)
                    ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),"你真的超爱 PVP……恭喜你已经把系列赛PVP通行证刷满了" +
                                                                          "\n无限等级上限为 100 个未领取奖励" +
                                                                          "\n在领取这些奖励之前你不会获得额外的系列赛经验！");

                ImGui.Spacing();
                ImGui.Separator();

                // Crystalline Conflict Section
                ImGui.TextColored(new Vector4(0.6f, 0.8f, 1f, 1f), "水晶冲突");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("胜利：900 系列赛经验" +
                        "\n失败：700 系列赛经验");
                    ImGui.EndTooltip();
                }
                ImGui.Spacing();
                ImGui.BulletText($"胜利：{xpResult.CrystallineConflictWin} " + (xpResult.CrystallineConflictWin == 1 ? "场" : "场"));
                ImGui.BulletText($"失败：{xpResult.CrystallineConflictLose} " + (xpResult.CrystallineConflictLose == 1 ? "场" : "场"));
                ImGui.Bullet();
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(1.0f, 0.84f, 0.0f, 1.0f),
                                  $"预计：{xpResult.CrystallineConflictExpectedMatches} " +
                                  (xpResult.CrystallineConflictExpectedMatches == 1 ? "场" : "场"));

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("按 50% 胜 / 50% 负的期望值计算");
                    ImGui.EndTooltip();
                }

                ImGui.Spacing();
                ImGui.Separator();

                // Frontlines Section
                ImGui.TextColored(new Vector4(0.8f, 0.6f, 0.6f, 1f), "纷争前线");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("第 1 名：1500 系列赛经验" +
                        "\n第 2 名：1250 系列赛经验" +
                        "\n第 3 名：1000 系列赛经验");
                    ImGui.EndTooltip();
                }
                ImGui.Spacing();
                ImGui.BulletText($"第 1 名：{xpResult.FrontlineWin} " + (xpResult.FrontlineWin == 1 ? "场" : "场"));
                ImGui.BulletText($"第 2 名：{xpResult.FrontlineLose2nd} " + (xpResult.FrontlineLose2nd == 1 ? "场" : "场"));
                ImGui.BulletText($"第 3 名：{xpResult.FrontlineLose3rd} " + (xpResult.FrontlineLose3rd == 1 ? "场" : "场"));
                ImGui.Bullet();
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(1.0f, 0.84f, 0.0f, 1.0f),
                                  $"预计：{xpResult.FrontlineExpectedMatches} " +
                                  (xpResult.FrontlineExpectedMatches == 1 ? "场" : "场"));

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("按三种名次各 1/3 概率的期望值计算");
                    ImGui.EndTooltip();
                }

                // Frontlines Roulette Section
                ImGui.TextColored(new Vector4(0.8f, 0.6f, 0.6f, 1f), "纷争前线（每日挑战）");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("在纷争前线奖励的基础上额外获得 1500 系列赛经验（每日一次）");
                    ImGui.EndTooltip();
                }
                ImGui.Spacing();
                ImGui.BulletText($"第 1 名：{xpResult.FrontlineDailyWin} " + (xpResult.FrontlineDailyWin == 1 ? "场" : "场"));
                ImGui.BulletText($"第 2 名：{xpResult.FrontlineDailyLose2nd} " + (xpResult.FrontlineDailyLose2nd == 1 ? "场" : "场"));
                ImGui.BulletText($"第 3 名：{xpResult.FrontlineDailyLose3rd} " + (xpResult.FrontlineDailyLose3rd == 1 ? "场" : "场"));
                ImGui.Bullet();
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(1.0f, 0.84f, 0.0f, 1.0f),
                                  $"预计：{xpResult.FrontlineDailyExpectedMatches} " +
                                  (xpResult.FrontlineDailyExpectedMatches == 1 ? "场/天" : "场/天"));

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("按三种名次各 1/3 概率的期望值计算。\n也表示你每天只打一次纷争前线、达到目标所需的最少天数。");
                    ImGui.EndTooltip();
                }


                if (Plugin.Configuration.TrackFrontlineBonus)
                {
                    if (Plugin.PvPService.CurrentFrontlineLosingBonus == -1)
                    {
                        ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "完成一场纷争前线以查看当前的报酬补正");
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text("用于计算你在纷争前线中连续获得第 3 名后获得的报酬补正" +
                                "\n打一局纷争前线以确认你现有的报酬补正" +
                                "\n可以在设置中关闭补正追踪");
                            ImGui.EndTooltip();
                        }
                    }
                    else
                    {
                        if(Plugin.PvPService.CurrentFrontlineLosingBonus == 0)
                        {
                            if(Plugin.PvPService.ConsecutiveThirdPlaceFrontline == 1)
                            {
                                ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "补正预备中：若本局获得第 3 名，将获得 10% 报酬补正");
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text("你的报酬补正已经预备好了！再次获得第 3 名即可获得 10% 报酬补正" +
                                    "\n若获得第 1 名则重置计数");
                                ImGui.EndTooltip();
                            }
                            ImGui.Text("当前没有激活任何纷争前线报酬补正");
                        }
                        else
                        {
                            if (Plugin.PvPService.CurrentFrontlineLosingBonus != 50)
                                ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "在获得第 1 名或第 2 名后，你将获得 " + Plugin.PvPService.CurrentFrontlineLosingBonus + "% 报酬补正");
                            else
                                ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "在获得第 1、2、3 名后，你将获得 " + Plugin.PvPService.CurrentFrontlineLosingBonus + "% 报酬补正");
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text("你将对系列赛经验、对战经验和狼印战绩按一定百分比获得补正，" +
                                    "直到获得一次第 1 名为止" );
                                ImGui.EndTooltip();
                            }
                            if (Plugin.PvPService.CurrentFrontlineLosingBonus != 50)
                            {
                                ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "若本局获得第 3 名，报酬补正将提升至 " + (Plugin.PvPService.CurrentFrontlineLosingBonus + 10) + "%");
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.BeginTooltip();
                                    ImGui.Text($"再次获得第 3 名将使补正提升至 {Plugin.PvPService.CurrentFrontlineLosingBonus + 10}%" +
                                                   "\n这一更高的补正也会应用于发生这一事件的当局");
                                    ImGui.EndTooltip();
                                }
                            }
                        }
                        if (Plugin.Configuration.OutdatedFrontlineRewardBonus)
                        {
                            ImGui.SameLine();
                            ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),"（数据已过期）");
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text("该信息可能已过期，原因是纷争前线追踪已经卸载！" +
                                    "\n将在你下一场纷争前线后重新刷新数据");
                                ImGui.EndTooltip();
                            }
                        }
                    }
                }

                ImGui.Spacing();
                ImGui.Separator();

                // Rival Wings Section
                ImGui.TextColored(new Vector4(0.6f, 0.8f, 0.6f, 1f), "烈羽争锋");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("胜利：1250 系列赛经验" +
                        "\n失败：750 系列赛经验");
                    ImGui.EndTooltip();
                }
                ImGui.Spacing();
                ImGui.BulletText($"胜利：{xpResult.RivalWingsWin} " + (xpResult.RivalWingsWin == 1 ? "场" : "场"));
                ImGui.BulletText($"失败：{xpResult.RivalWingsLose} " + (xpResult.RivalWingsLose == 1 ? "场" : "场"));
                ImGui.Bullet();
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(1.0f, 0.84f, 0.0f, 1.0f),
                                  $"预计：{xpResult.RivalWingsExpectedMatches} " +
                                  (xpResult.RivalWingsExpectedMatches == 1 ? "场" : "场"));

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("按 50% 胜 / 50% 负的期望值计算");
                    ImGui.EndTooltip();
                }

                ImGui.Separator();
                ImGui.Spacing();
                if (ImGui.Button("设置"))
                    Plugin.ToggleConfigUI();
                ImGui.SameLine();
                if (pvpInfo.CurrentSeriesRank != pvpInfo.ClaimedSeriesRank)
                {
                    ImGui.Text("别忘了去领取你的系列赛PVP通行证奖励！");
                }

            }
            else
            {
                ImGui.Text("对战资料尚未加载。");
            }
        }
        public void OnOpenPVPRewardWindow()
        {
            IsOpen = true;
        }

        public void OnClosePVPRewardWindow()
        {
            IsOpen = false;
        }

    }
}
