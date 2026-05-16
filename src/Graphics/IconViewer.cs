using ImGuiNET;
using MiniAudioPlayer.Embedded;
using System.Numerics;
using System.Reflection;

namespace MiniAudioPlayer.Graphics
{
    public class IconViewer
    {
        private FieldInfo[] fields;
        private string filterText = "";

        public IconViewer()
        {
            fields = typeof(FontAwesome).GetFields(BindingFlags.Public | BindingFlags.Static);
        }

        public void Draw()
        {
            if (ImGui.Begin("FontAwesome Icon Viewer"))
            {
                ImGui.InputText("Filter", ref filterText, 100);

                // Define how many columns you want in the grid
                const int numColumns = 4;

                if (ImGui.BeginTable("IconGrid", numColumns, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedSame))
                {
                    // Setup column headers or just fixed widths
                    for (int i = 0; i < numColumns; i++)
                    {
                        ImGui.TableSetupColumn($"Col {i}", ImGuiTableColumnFlags.WidthFixed, 250.0f);
                    }

                    int columnCount = 0;

                    foreach (FieldInfo field in fields)
                    {
                        if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                        {
                            if (!field.Name.StartsWith("ICON_FA_"))
                            {
                                continue;
                            }

                            string iconValue = field.GetRawConstantValue() as string;
                            if (!string.IsNullOrEmpty(iconValue))
                            {
                                if (columnCount == 0)
                                {
                                    ImGui.TableNextRow();
                                }

                                ImGui.TableSetColumnIndex(columnCount);

                                string cleanName = field.Name.Replace("ICON_FA_", "");

                                bool isMatch = !string.IsNullOrWhiteSpace(filterText) && cleanName.Contains(filterText, System.StringComparison.OrdinalIgnoreCase);
                                Vector4 nameColor = isMatch ? new Vector4(1.0f, 0.0f, 0.0f, 1.0f) : new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

                                // Render the icon and label inline inside the cell
                                ImGui.TextColored(new Vector4(0.2f, 0.7f, 1.0f, 1.0f), iconValue);
                                ImGui.SameLine();

                                ImGui.TextColored(nameColor, cleanName);

                                columnCount++;
                                if (columnCount >= numColumns)
                                {
                                    columnCount = 0;
                                }
                            }
                        }
                    }

                    ImGui.EndTable();
                }
                ImGui.End();
            }
        }
    }
}