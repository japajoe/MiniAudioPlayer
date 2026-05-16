using System;
using System.Runtime.InteropServices;
using ImGuiNET;
using MiniAudioPlayer.Embedded;
using OpenTK.Windowing.Desktop;

namespace MiniAudioPlayer.Graphics
{
    public sealed class ImGuiController : IDisposable
    {
        private NativeWindow window;
        private IntPtr pIconRanges;

        public ImGuiController(NativeWindow window)
        {
            this.window = window;

            ImGui.CreateContext();
            ImGuiIOPtr io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            //io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

            ImGui.StyleColorsDark();

            ImGuiStylePtr style = ImGui.GetStyle();
            if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
            {
                style.WindowRounding = 0.0f;
                style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
            }

            io.Fonts.AddFontDefault();

            unsafe
            {
                ImFontConfigPtr configuration = ImGuiNative.ImFontConfig_ImFontConfig();
                configuration.MergeMode = true;
                configuration.GlyphMinAdvanceX = 13.0f;
                configuration.FontDataOwnedByAtlas = false;

                pIconRanges = Marshal.AllocHGlobal(3 * sizeof(ushort));
                ushort *ranges = (ushort*)pIconRanges;
                ranges[0] = FontAwesome.ICON_MIN_FA;
                ranges[1] = FontAwesome.ICON_MAX_FA;
                ranges[2] = 0;

                byte[] fontData = FontAwesome.GetFaSolid900Data();
                IntPtr pFontData = Marshal.AllocHGlobal(fontData.Length);
                Marshal.Copy(fontData, 0, pFontData, fontData.Length);

                try
                {
                    io.Fonts.AddFontFromMemoryTTF(pFontData, fontData.Length, 13.0f, configuration, pIconRanges);
                }
                finally
                {
                    configuration.Destroy();
                    Marshal.FreeHGlobal(pFontData);
                }
            }

            ImguiImplOpenTK5.Init(window);
            ImguiImplOpenGL3.Init();
        }

        public void NewFrame()
        {
            ImguiImplOpenGL3.NewFrame();
            ImguiImplOpenTK5.NewFrame();
            ImGui.NewFrame();
        }

        public void Dispose()
        {
            ImguiImplOpenGL3.Shutdown();
            ImguiImplOpenTK5.Shutdown();
            Marshal.FreeHGlobal(pIconRanges);
        }

        public void EndFrame()
        {
            ImGui.Render();

            ImguiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());
            
            if (ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
            {
                ImGui.UpdatePlatformWindows();
                ImGui.RenderPlatformWindowsDefault();
                window.Context.MakeCurrent();
            }
        }
    }
}