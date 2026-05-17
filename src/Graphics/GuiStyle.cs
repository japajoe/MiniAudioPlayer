using System.Numerics;
using ImGuiNET;

namespace MiniAudioPlayer
{
    public static class GuiStyle
    {
        public static void UseCatppuccinMochaStyle()
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            var colors = style.Colors;

            // --- 1. Sizing and Spacing (Soft & Modern) ---
            style.WindowPadding = new Vector2(12.0f, 12.0f);
            style.FramePadding = new Vector2(6.0f, 4.0f);
            style.ItemSpacing = new Vector2(8.0f, 6.0f);
            style.ScrollbarSize = 14.0f;
            style.GrabMinSize = 12.0f;

            // --- 2. Borders & Rounding ---
            style.WindowRounding = 8.0f;
            style.FrameRounding = 5.0f;
            style.PopupRounding = 5.0f;
            style.ScrollbarRounding = 12.0f;
            style.GrabRounding = 5.0f;
            style.TabRounding = 5.0f;

            style.WindowBorderSize = 1.0f;
            style.FrameBorderSize = 0.0f; // Minimalist look
            style.PopupBorderSize = 1.0f;

            // --- 3. The Catppuccin Mocha Palette ---
            // Base: #1e1e2e | Mantle: #181825 | Crust: #11111b
            // Text: #cdd6f4 | Subtext0: #a6adc8 | Surface0: #313244
            // Lavender: #b4befe | Sapphire: #74c7ec | Mauve: #cba6f7

            // Text
            colors[(int)ImGuiCol.Text] = new Vector4(0.80f, 0.84f, 0.96f, 1.00f); // Text
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.42f, 0.45f, 0.55f, 1.00f); // Surface1

            // Backgrounds
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.12f, 0.12f, 0.18f, 1.00f); // Base
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.09f, 0.09f, 0.15f, 1.00f); // Mantle
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.07f, 0.07f, 0.11f, 0.96f); // Crust

            // Borders
            colors[(int)ImGuiCol.Border] = new Vector4(0.19f, 0.20f, 0.27f, 1.00f); // Surface0
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);

            // Frames (Inputs, etc.)
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.19f, 0.20f, 0.27f, 1.00f); // Surface0
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.25f, 0.26f, 0.35f, 1.00f); // Surface1
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.31f, 0.32f, 0.42f, 1.00f); // Surface2

            // Title Bars
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.09f, 0.09f, 0.15f, 1.00f); // Mantle
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.12f, 0.12f, 0.18f, 1.00f); // Base
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.07f, 0.07f, 0.11f, 1.00f); // Crust

            // Menus
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.09f, 0.09f, 0.15f, 1.00f);

            // Scrollbars
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.09f, 0.09f, 0.15f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.32f, 0.42f, 1.00f); // Surface2
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.37f, 0.38f, 0.51f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.42f, 0.45f, 0.55f, 1.00f);

            // Interactables
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.71f, 0.75f, 1.00f, 1.00f); // Lavender
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.45f, 0.78f, 0.93f, 1.00f); // Sapphire
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.45f, 0.78f, 0.93f, 1.00f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.19f, 0.20f, 0.27f, 1.00f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.80f, 0.65f, 0.97f, 1.00f); // Mauve
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.70f, 0.55f, 0.87f, 1.00f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.297f, 0.310f, 0.403f, 1.00f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.25f, 0.26f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.31f, 0.32f, 0.42f, 1.00f);

            // Tabs
            colors[(int)ImGuiCol.Tab] = new Vector4(0.12f, 0.12f, 0.18f, 1.00f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.31f, 0.32f, 0.42f, 1.00f);
            //colors[(int)ImGuiCol.TabActive] = new Vector4(0.19f, 0.20f, 0.27f, 1.00f);
            //colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.09f, 0.09f, 0.15f, 1.00f);
            //colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.12f, 0.12f, 0.18f, 1.00f);

            // Misc
            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.94f, 0.72f, 0.42f, 1.00f); // Marigold
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.31f, 0.32f, 0.42f, 1.00f);
            //colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.71f, 0.75f, 1.00f, 1.00f); // Lavender

            colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.71f, 0.75f, 1.00f, 0.50f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.12f, 0.12f, 0.18f, 1.00f);
        }
    }
}