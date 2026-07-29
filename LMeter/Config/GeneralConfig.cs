using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using LMeter.Helpers;
using Newtonsoft.Json;

namespace LMeter.Config
{
    public class GeneralConfig : IConfigPage
    {
        [JsonIgnore]
        private static readonly string[] m_meterTypeOptions = ["傷害", "治療", "承受傷害"];

        [JsonIgnore]
        public bool Preview = false;

        [JsonIgnore]
        public bool Active { get; set; }

        public string Name => "一般";
        public Vector2 Position = Vector2.Zero;
        public Vector2 Size = new(ImGui.GetMainViewport().Size.Y * 16 / 90, ImGui.GetMainViewport().Size.Y / 10);
        public bool Lock = false;
        public bool ClickThrough = false;
        public ConfigColor BackgroundColor = new(0, 0, 0, 0.5f);
        public bool ShowBorder = true;
        public bool BorderAroundBars = false;
        public ConfigColor BorderColor = new(30f / 255f, 30f / 255f, 30f / 255f, 230f / 255f);
        public int BorderThickness = 2;
        public MeterDataType DataType = MeterDataType.Damage;
        public bool ReturnToCurrent = true;
        public RoundingOptions Rounding = new(false, 10f, RoundingFlag.Bottom);
        public RoundingOptions BorderRounding = new(false, 10f, RoundingFlag.All);

        public IConfigPage GetDefault() => new GeneralConfig();

        public void DrawConfig(Vector2 size, float padX, float padY, bool border = true)
        {
            if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), border))
            {
                Vector2 screenSize = ImGui.GetMainViewport().Size;
                ImGui.DragFloat2("位置", ref this.Position, 1, -screenSize.X / 2, screenSize.X / 2);
                ImGui.DragFloat2("大小", ref this.Size, 1, 0, screenSize.Y);
                ImGui.Checkbox("鎖定", ref this.Lock);
                ImGui.Checkbox("允許點擊穿透", ref this.ClickThrough);
                ImGui.Checkbox("預覽", ref this.Preview);
                ImGui.NewLine();

                DrawHelpers.DrawColorSelector("背景顏色", this.BackgroundColor);
                DrawHelpers.DrawRoundingOptions("使用圓角", 0, this.Rounding);

                ImGui.NewLine();
                ImGui.Checkbox("顯示邊框", ref this.ShowBorder);
                if (this.ShowBorder)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragInt("邊框粗細", ref this.BorderThickness, 1, 1, 20);

                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawColorSelector("邊框顏色", this.BorderColor);

                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawRoundingOptions("使用圓角##Border", 1, this.BorderRounding);

                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("隱藏標題列周圍的邊框", ref this.BorderAroundBars);
                }

                ImGui.NewLine();
                ImGui.Combo(
                    "統計類型",
                    ref Unsafe.As<MeterDataType, int>(ref this.DataType),
                    m_meterTypeOptions,
                    m_meterTypeOptions.Length
                );

                ImGui.Checkbox("進入戰鬥時返回目前資料", ref this.ReturnToCurrent);
            }

            ImGui.EndChild();
        }
    }
}
