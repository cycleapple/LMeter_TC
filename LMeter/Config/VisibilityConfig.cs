using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using LMeter.Act;
using LMeter.Helpers;
using Newtonsoft.Json;

namespace LMeter.Config
{
    public class VisibilityConfig : IConfigPage
    {
        [JsonIgnore]
        public static readonly string[] OperatorOptions = new string[] { "且", "或", "互斥或" };

        [JsonIgnore]
        public static readonly string[] ResultOptions = new string[] { "顯示", "隱藏" };

        [JsonIgnore]
        private int m_swapX = -1;

        [JsonIgnore]
        private int m_swapY = -1;

        [JsonIgnore]
        private int m_selectedIndex = 0;

        [JsonIgnore]
        public bool Active { get; set; }

        public string Name => "顯示條件";

        public List<VisibilityCondition> VisibilityConditions = new List<VisibilityCondition>();
        public bool Initialized = false;
        public bool AlwaysHide = false;
        public bool ShouldClip = true;
        public bool ShowOnMouseover = false;
        public bool HideIfNotConnected = false;
        public int ResultOption = 0;

        public VisibilityConfig()
        {
            this.VisibilityConditions =  new List<VisibilityCondition>();
        }

        public IConfigPage GetDefault() => new VisibilityConfig();

        public bool IsVisible()
        {
            if (this.VisibilityConditions.Count == 0)
            {
                return false;
            }

            if (this.AlwaysHide)
            {
                return false;
            }

            if (this.HideIfNotConnected && Singletons.Get<LogClient>().Status != ConnectionStatus.Connected)
            {
                return false;
            }

            bool active = this.VisibilityConditions[0].IsActive();
            for (int i = 1; i < this.VisibilityConditions.Count; i++)
            {
                VisibilityCondition option = this.VisibilityConditions[i];
                bool currentActive = option.IsActive();

                active = option.Operator switch
                {
                    BooleanOperator.And => active && currentActive,
                    BooleanOperator.Or => active || currentActive,
                    BooleanOperator.Xor => active ^ currentActive,
                    _ => false,
                };
            }

            return active ^ this.ResultOption == 1;
        }

        public void DrawConfig(Vector2 size, float padX, float padY, bool border = true)
        {
            if (this.VisibilityConditions.Count == 0)
            {
                this.AddOption(new());
            }

            float posY = ImGui.GetCursorPosY();
            ImGui.Checkbox("永遠隱藏", ref this.AlwaysHide);
            ImGui.Checkbox("被遊戲介面遮住時隱藏", ref this.ShouldClip);
            ImGui.Checkbox("滑鼠指向時永遠顯示", ref this.ShowOnMouseover);
            ImGui.Checkbox("未連線至 ACT 時永遠隱藏", ref this.HideIfNotConnected);
            size = new(size.X, size.Y - (ImGui.GetCursorPosY() - posY));

            if (ImGui.BeginChild("##VisibilityOptionConfig", new Vector2(size.X, size.Y), border))
            {
                ImGui.Text("顯示條件");

                ImGuiTableFlags tableFlags =
                    ImGuiTableFlags.RowBg
                    | ImGuiTableFlags.Borders
                    | ImGuiTableFlags.BordersOuter
                    | ImGuiTableFlags.BordersInner
                    | ImGuiTableFlags.ScrollY
                    | ImGuiTableFlags.NoSavedSettings;

                if (
                    ImGui.BeginTable(
                        "##VisibilityOptions_Table",
                        4,
                        tableFlags,
                        new Vector2(size.X - padX * 2, (size.Y - ImGui.GetCursorPosY() - padY * 2) / 4)
                    )
                )
                {
                    Vector2 buttonSize = new(30, 0);
                    int buttonCount = this.VisibilityConditions.Count > 1 ? 5 : 3;
                    float actionsWidth = buttonSize.X * buttonCount + padX * (buttonCount - 1);
                    ImGui.TableSetupColumn("運算", ImGuiTableColumnFlags.WidthFixed, 60, 0);
                    ImGui.TableSetupColumn("反向", ImGuiTableColumnFlags.WidthFixed, 35, 1);
                    ImGui.TableSetupColumn("條件類型", ImGuiTableColumnFlags.WidthStretch, 0, 2);
                    ImGui.TableSetupColumn("操作", ImGuiTableColumnFlags.WidthFixed, actionsWidth, 3);
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < this.VisibilityConditions.Count; i++)
                    {
                        ImGui.PushID(i.ToString());
                        ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);
                        this.DrawOptionsRow(i);
                    }

                    ImGui.PushID(this.VisibilityConditions.Count.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);
                    ImGui.TableSetColumnIndex(3);
                    DrawHelpers.DrawButton(
                        string.Empty,
                        FontAwesomeIcon.Plus,
                        () => this.AddOption(),
                        "新增條件",
                        buttonSize
                    );
                    ImGui.SameLine();
                    DrawHelpers.DrawButton(
                        string.Empty,
                        FontAwesomeIcon.Download,
                        () => this.ImportOption(),
                        "匯入條件",
                        buttonSize
                    );

                    ImGui.EndTable();

                    if (
                        m_swapX < this.VisibilityConditions.Count
                        && m_swapX >= 0
                        && m_swapY < this.VisibilityConditions.Count
                        && m_swapY >= 0
                    )
                    {
                        VisibilityCondition temp = this.VisibilityConditions[m_swapX];
                        this.VisibilityConditions[m_swapX] = this.VisibilityConditions[m_swapY];
                        this.VisibilityConditions[m_swapY] = temp;

                        m_swapX = -1;
                        m_swapY = -1;
                    }
                }

                ImGui.Text("條件成立時：");
                ImGui.SameLine();
                ImGui.PushItemWidth(100);
                ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(0f, -2f));
                ImGui.Combo("##ResultCombo", ref this.ResultOption, ResultOptions, ResultOptions.Length);
                ImGui.PopItemWidth();

                ImGui.Text($"編輯條件 {m_selectedIndex + 1}");
                if (
                    ImGui.BeginChild(
                        "##ConditionEdit",
                        new Vector2(size.X - padX * 2, size.Y - ImGui.GetCursorPosY() - padY * 2),
                        true
                    )
                )
                {
                    VisibilityCondition selectedOption = this.VisibilityConditions[m_selectedIndex];
                    selectedOption.DrawConfig(ImGui.GetWindowSize(), padX, padX);

                    ImGui.EndChild();
                }

                ImGui.EndChild();
            }
        }

        private void DrawOptionsRow(int i)
        {
            if (i >= this.VisibilityConditions.Count)
            {
                return;
            }

            VisibilityCondition condition = this.VisibilityConditions[i];
            if (ImGui.TableSetColumnIndex(0))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (i == 0 ? 3f : 1f));
                if (i == 0)
                {
                    ImGui.Text("若");
                }
                else
                {
                    ImGui.PushItemWidth(ImGui.GetColumnWidth());
                    ImGui.Combo(
                        "##CondCombo",
                        ref Unsafe.As<BooleanOperator, int>(ref condition.Operator),
                        OperatorOptions,
                        OperatorOptions.Length
                    );
                    ImGui.PopItemWidth();
                }
            }

            if (ImGui.TableSetColumnIndex(1))
            {
                ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(6f, 1f));
                ImGui.Checkbox(string.Empty, ref condition.Inverted);
            }

            if (ImGui.TableSetColumnIndex(2))
            {
                ImGui.Text(
                    VisibilityCondition.GetConditionDescription(condition)
                );
            }

            if (ImGui.TableSetColumnIndex(3))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                Vector2 buttonSize = new(30, 0);
                DrawHelpers.DrawButton(
                    string.Empty,
                    FontAwesomeIcon.Pen,
                    () => this.SelectOption(i),
                    "編輯條件",
                    buttonSize
                );

                if (this.VisibilityConditions.Count > 1)
                {
                    ImGui.SameLine();
                    DrawHelpers.DrawButton(
                        string.Empty,
                        FontAwesomeIcon.ArrowUp,
                        () => this.Swap(i, i - 1),
                        "上移",
                        buttonSize
                    );

                    ImGui.SameLine();
                    DrawHelpers.DrawButton(
                        string.Empty,
                        FontAwesomeIcon.ArrowDown,
                        () => this.Swap(i, i + 1),
                        "下移",
                        buttonSize
                    );
                }

                ImGui.SameLine();
                DrawHelpers.DrawButton(
                    string.Empty,
                    FontAwesomeIcon.Upload,
                    () => this.ExportOption(i),
                    "匯出條件",
                    buttonSize
                );
                if (this.VisibilityConditions.Count > 1)
                {
                    ImGui.SameLine();
                    DrawHelpers.DrawButton(
                        string.Empty,
                        FontAwesomeIcon.Trash,
                        () => this.RemoveOption(i),
                        "移除條件",
                        buttonSize
                    );
                }
            }
        }

        private void SelectOption(int i)
        {
            m_selectedIndex = i;
        }

        private void AddOption(VisibilityCondition? newOption = null)
        {
            this.VisibilityConditions.Add(newOption ?? new VisibilityCondition());
            this.SelectOption(this.VisibilityConditions.Count - 1);
        }

        private void ExportOption(int i)
        {
            if (i < this.VisibilityConditions.Count && i >= 0)
            {
                ConfigHelpers.ExportToClipboard(this.VisibilityConditions[i]);
            }
        }

        private void ImportOption()
        {
            string importString = ImGui.GetClipboardText();
            if (!string.IsNullOrEmpty(importString))
            {
                VisibilityCondition? newOption = ConfigHelpers.GetFromImportString<VisibilityCondition>(importString);
                if (newOption is not null)
                {
                    this.AddOption(newOption);
                }
            }
        }

        private void RemoveOption(int i)
        {
            if (i < this.VisibilityConditions.Count && i >= 0)
            {
                this.VisibilityConditions.RemoveAt(i);
                m_selectedIndex = Math.Clamp(m_selectedIndex, 0, this.VisibilityConditions.Count - 1);
            }
        }

        private void Swap(int x, int y)
        {
            m_swapX = x;
            m_swapY = y;
        }
    }

    public class VisibilityCondition
    {
        [JsonIgnore]
        private string m_customJobInput = string.Empty;

        public bool Inverted = false;
        public BooleanOperator Operator = BooleanOperator.And;
        public VisibilityConditionType ConditionType = VisibilityConditionType.AlwaysTrue;

        public JobType ShowForJobTypes = JobType.All;
        public string CustomJobString = string.Empty;
        public List<Job> CustomJobList = new List<Job>();

        public ZoneType Zone;

        public bool IsActive() =>
            this.Inverted
            ^ this.ConditionType switch
            {
                VisibilityConditionType.AlwaysTrue => true,
                VisibilityConditionType.InCombat => CharacterState.IsInCombat(),
                VisibilityConditionType.InDuty => CharacterState.IsInDuty(),
                VisibilityConditionType.Performing => CharacterState.IsPerforming(),
                VisibilityConditionType.Zone => CharacterState.InZone(this.Zone),
                VisibilityConditionType.Job => CharacterState.IsJobType(
                    CharacterState.GetCharacterJob(),
                    this.ShowForJobTypes,
                    this.CustomJobList
                ),
                _ => true,
            };

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            ImGui.RadioButton(
                "永遠成立",
                ref Unsafe.As<VisibilityConditionType, int>(ref this.ConditionType),
                (int)VisibilityConditionType.AlwaysTrue
            );
            ImGui.RadioButton(
                "戰鬥中",
                ref Unsafe.As<VisibilityConditionType, int>(ref this.ConditionType),
                (int)VisibilityConditionType.InCombat
            );
            ImGui.RadioButton(
                "任務中",
                ref Unsafe.As<VisibilityConditionType, int>(ref this.ConditionType),
                (int)VisibilityConditionType.InDuty
            );
            ImGui.RadioButton(
                "演奏中",
                ref Unsafe.As<VisibilityConditionType, int>(ref this.ConditionType),
                (int)VisibilityConditionType.Performing
            );

            ImGui.RadioButton(
                "位於指定區域",
                ref Unsafe.As<VisibilityConditionType, int>(ref this.ConditionType),
                (int)VisibilityConditionType.Zone
            );
            if (this.ConditionType == VisibilityConditionType.Zone)
            {
                DrawHelpers.DrawNestIndicator(1);
                ImGui.RadioButton("金碟遊樂場", ref Unsafe.As<ZoneType, int>(ref this.Zone), (int)ZoneType.GoldSaucer);
                DrawHelpers.DrawNestIndicator(1);
                ImGui.RadioButton(
                    "玩家房屋",
                    ref Unsafe.As<ZoneType, int>(ref this.Zone),
                    (int)ZoneType.PlayerHouse
                );
            }

            ImGui.RadioButton(
                "職業",
                ref Unsafe.As<VisibilityConditionType, int>(ref this.ConditionType),
                (int)VisibilityConditionType.Job
            );
            if (this.ConditionType == VisibilityConditionType.Job)
            {
                DrawHelpers.DrawNestIndicator(1);
                string[] jobTypeOptions = ["全部", "自訂", "防護職業", "魔法遠程職業", "近戰職業", "物理遠程職業", "治療職業", "戰鬥精英", "魔法導師", "戰鬥職業", "生產職業", "能工巧匠", "大地使者"];
                ImGui.Combo(
                    "選擇職業類型",
                    ref Unsafe.As<JobType, int>(ref this.ShowForJobTypes),
                    jobTypeOptions,
                    jobTypeOptions.Length
                );

                if (this.ShowForJobTypes == JobType.Custom)
                {
                    if (string.IsNullOrEmpty(m_customJobInput))
                    {
                        m_customJobInput = this.CustomJobString.ToUpper();
                    }

                    DrawHelpers.DrawNestIndicator(1);
                    if (
                        ImGui.InputTextWithHint(
                            "自訂職業清單",
                            "以逗號分隔（例如：WAR, SAM, BLM）",
                            ref m_customJobInput,
                            100,
                            ImGuiInputTextFlags.EnterReturnsTrue
                        )
                    )
                    {
                        IEnumerable<string> jobStrings = m_customJobInput.Split(',').Select(j => j.Trim());
                        List<Job> jobList = new List<Job>();
                        foreach (string j in jobStrings)
                        {
                            if (Enum.TryParse(j, true, out Job parsed))
                            {
                                jobList.Add(parsed);
                            }
                            else
                            {
                                jobList.Clear();
                                m_customJobInput = string.Empty;
                                break;
                            }
                        }

                        m_customJobInput = m_customJobInput.ToUpper();
                        this.CustomJobString = m_customJobInput;
                        this.CustomJobList = jobList;
                    }
                }
            }
        }

        internal static string GetConditionDescription(VisibilityCondition condition) => condition.ConditionType switch
        {
            VisibilityConditionType.AlwaysTrue => "永遠成立",
            VisibilityConditionType.InCombat => "戰鬥中",
            VisibilityConditionType.InDuty => "任務中",
            VisibilityConditionType.Performing => "演奏中",
            VisibilityConditionType.Zone => condition.Zone switch
            {
                ZoneType.GoldSaucer => "區域：金碟遊樂場",
                ZoneType.PlayerHouse => "區域：玩家房屋",
                _ => "指定區域",
            },
            VisibilityConditionType.Job => "職業",
            _ => condition.ConditionType.ToString(),
        };
    }
}
