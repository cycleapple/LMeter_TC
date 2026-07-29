using System.Numerics;
using System.Text.Json.Serialization;
using Dalamud.Bindings.ImGui;
using LMeter.Helpers;

namespace LMeter.Config
{
    public class HeaderConfig : IConfigPage
    {
        [JsonIgnore]
        public bool Active { get; set; }

        public string Name => "標題列／頁尾";

        public bool ShowHeader = true;
        public int HeaderHeight = 25;
        public ConfigColor BackgroundColor = new(30f / 255f, 30f / 255f, 30f / 255f, 230 / 255f);

        public bool ShowFooter = false;
        public int FooterHeight = 25;
        public ConfigColor FooterBackgroundColor = new(0, 0, 0, .5f);

        public bool ShowEncounterDuration = true;
        public ConfigColor DurationColor = new(0f / 255f, 190f / 255f, 225f / 255f, 1f);
        public bool DurationShowOutline = true;
        public ConfigColor DurationOutlineColor = new(0, 0, 0, 0.5f);
        public DrawAnchor DurationAlign = DrawAnchor.Left;
        public Vector2 DurationOffset = new(0, 0);
        public int DurationFontId = 0;
        public string DurationFontKey = FontsManager.DefaultSmallFontKey;

        public bool ShowEncounterName = true;
        public ConfigColor NameColor = new(1, 1, 1, 1);
        public bool NameShowOutline = true;
        public ConfigColor NameOutlineColor = new(0, 0, 0, 0.5f);
        public DrawAnchor NameAlign = DrawAnchor.Left;
        public Vector2 NameOffset = new(0, 0);
        public int NameFontId = 0;
        public string NameFontKey = FontsManager.DefaultSmallFontKey;

        public bool ShowRaidStats = true;
        public ConfigColor RaidStatsColor = new(0.5f, 0.5f, 0.5f, 1f);
        public bool StatsShowOutline = true;
        public ConfigColor StatsOutlineColor = new(0, 0, 0, 0.5f);
        public DrawAnchor StatsAlign = DrawAnchor.Right;
        public Vector2 StatsOffset = new(0, 0);
        public int StatsFontId = 0;
        public string StatsFontKey = FontsManager.DefaultSmallFontKey;
        public string RaidStatsFormat = "[dps]rdps [hps]rhps Deaths: [deaths]";
        public bool ThousandsSeparators = true;

        public bool ShowVersion = true;
        public Vector2 VersionOffset = new(0, 0);
        public int VersionFontId = 0;
        public ConfigColor VersionColor = new(0f / 255f, 190f / 255f, 225f / 255f, 1f);
        public bool VersionShowOutline = true;
        public ConfigColor VersionOutlineColor = new(0, 0, 0, 0.5f);
        public string VersionFontKey = FontsManager.DefaultSmallFontKey;

        public RoundingOptions Rounding = new(false, 10f, RoundingFlag.Top);

        public IConfigPage GetDefault()
        {
            HeaderConfig defaultConfig = new()
            {
                DurationFontKey = FontsManager.DefaultSmallFontKey,
                DurationFontId = FontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),

                NameFontKey = FontsManager.DefaultSmallFontKey,
                NameFontId = FontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),

                StatsFontKey = FontsManager.DefaultSmallFontKey,
                StatsFontId = FontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),
            };

            return defaultConfig;
        }

        public void DrawConfig(Vector2 size, float padX, float padY, bool border = true)
        {
            if (ImGui.BeginChild($"##{this.Name}", size, border))
            {
                ImGui.Checkbox("顯示標題列", ref this.ShowHeader);
                if (this.ShowHeader)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragInt("高度##Header", ref this.HeaderHeight, 1, 0, 100);

                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawColorSelector("背景顏色##Header", this.BackgroundColor);

                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawRoundingOptions("使用圓角##Header", 1, this.Rounding);

                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("清除資料後顯示 LMeter 版本", ref this.ShowVersion);
                    if (this.ShowVersion)
                    {
                        DrawHelpers.DrawNestIndicator(2);
                        ImGui.DragFloat2("偏移##Version", ref this.VersionOffset);

                        DrawHelpers.DrawNestIndicator(2);
                        DrawHelpers.DrawFontSelector("字型##Version", ref this.VersionFontKey, ref this.VersionFontId);

                        DrawHelpers.DrawNestIndicator(2);
                        DrawHelpers.DrawColorSelector("文字顏色##Version", this.VersionColor);

                        DrawHelpers.DrawNestIndicator(2);
                        ImGui.Checkbox("顯示外框##Version", ref this.VersionShowOutline);
                        if (this.VersionShowOutline)
                        {
                            DrawHelpers.DrawNestIndicator(3);
                            DrawHelpers.DrawColorSelector("外框顏色##Version", this.VersionOutlineColor);
                        }
                    }

                    ImGui.NewLine();
                }

                ImGui.Checkbox("顯示頁尾", ref this.ShowFooter);
                if (this.ShowFooter)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragInt("高度##Footer", ref this.FooterHeight, 1, 0, 100);

                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawColorSelector("背景顏色##Footer", this.FooterBackgroundColor);
                }

                ImGui.EndChild();
            }
        }
    }
}
