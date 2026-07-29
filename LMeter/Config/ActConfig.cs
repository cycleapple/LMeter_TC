using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using LMeter.Act;
using LMeter.Helpers;
using Newtonsoft.Json;

namespace LMeter.Config
{
    public class ActConfig : IConfigPage
    {
        [JsonIgnore]
        private const string SOCKET_ADDRESS = "ws://127.0.0.1:10501/ws";

        [JsonIgnore]
        public bool Active { get; set; }
        public string Name => "ACT";
        public string ActSocketAddress;
        public int EncounterHistorySize = 15;
        public bool AutoReconnect = false;
        public int ReconnectDelay = 30;
        public bool ClearAct = false;
        public bool AutoEnd = false;
        public int AutoEndDelay = 3;
        public int ClientType = 0;
        public bool LogConnectionErrors = true;

        public ActConfig()
        {
            this.ActSocketAddress = SOCKET_ADDRESS;
        }

        public IConfigPage GetDefault() => new ActConfig();

        public void DrawConfig(Vector2 size, float padX, float padY, bool border = true)
        {
            if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), border))
            {
                int currentClientType = this.ClientType;
                ImGui.Text("ACT 連線類型：");
                ImGui.RadioButton("WebSocket", ref this.ClientType, 0);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(
                        "使用標準獨立版 Advanced Combat Tracker 時請選擇此項。"
                    );
                }

                ImGui.SameLine();
                ImGui.RadioButton("IINACT IPC", ref this.ClientType, 1);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("使用 Dalamud 的 IINACT 插件時請選擇此項。");
                }

                if (currentClientType != this.ClientType)
                {
                    Singletons.Get<PluginManager>().ChangeClientType(this.ClientType);
                }

                Vector2 buttonSize = new(40, 0);
                ImGui.Text($"ACT 狀態：{GetLocalizedStatus(Singletons.Get<LogClient>().Status)}");
                if (this.ClientType == 0)
                {
                    ImGui.InputTextWithHint(
                        "ACT WebSocket 位址",
                        $"預設：'{SOCKET_ADDRESS}'",
                        ref this.ActSocketAddress,
                        64
                    );
                }

                DrawHelpers.DrawButton(
                    string.Empty,
                    FontAwesomeIcon.Sync,
                    () => Singletons.Get<LogClient>().Reset(),
                    "重新連線",
                    buttonSize
                );

                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1f);
                ImGui.Text("重新嘗試 ACT 連線");

                ImGui.NewLine();
                ImGui.PushItemWidth(30);
                ImGui.InputInt("保留的戰鬥紀錄數量", ref this.EncounterHistorySize, 0, 0);
                ImGui.PopItemWidth();

                ImGui.NewLine();
                ImGui.Checkbox("連線失敗時自動嘗試重新連線", ref this.AutoReconnect);
                if (this.AutoReconnect)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.PushItemWidth(30);
                    ImGui.InputInt("重新連線嘗試間隔（秒）", ref this.ReconnectDelay, 0, 0);
                    ImGui.PopItemWidth();
                }

                ImGui.Checkbox("記錄連線錯誤", ref this.LogConnectionErrors);

                ImGui.NewLine();
                ImGui.Checkbox("清除 LMeter 時一併清除 ACT", ref this.ClearAct);
                ImGui.Checkbox("脫離戰鬥後強制 ACT 結束本次戰鬥", ref this.AutoEnd);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(
                        "使用此功能時，建議停用 ACT 的指令音效。\n"
                            + "可在 ACT 的「Options → Sound Settings」中調整。"
                    );
                }

                if (this.AutoEnd)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.PushItemWidth(30);
                    ImGui.InputInt("脫離戰鬥後延遲（秒）", ref this.AutoEndDelay, 0, 0);
                    ImGui.PopItemWidth();
                }

                ImGui.NewLine();
                DrawHelpers.DrawButton(
                    string.Empty,
                    FontAwesomeIcon.Stop,
                    () => Singletons.Get<LogClient>().EndEncounter(),
                    null,
                    buttonSize
                );
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1f);
                ImGui.Text("強制結束戰鬥");

                DrawHelpers.DrawButton(
                    string.Empty,
                    FontAwesomeIcon.Trash,
                    () => Singletons.Get<PluginManager>().Clear(),
                    null,
                    buttonSize
                );
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1f);
                ImGui.Text("清除 LMeter");
            }

            ImGui.EndChild();
        }

        private static string GetLocalizedStatus(ConnectionStatus status) => status switch
        {
            ConnectionStatus.Connected => "已連線",
            ConnectionStatus.Connecting => "連線中",
            ConnectionStatus.NotConnected => "未連線",
            ConnectionStatus.ShuttingDown => "正在關閉",
            ConnectionStatus.ConnectionFailed => "連線失敗",
            _ => status.ToString(),
        };
    }
}
