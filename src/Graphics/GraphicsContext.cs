// MIT License

// Copyright (c) 2025 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using MiniAudioPlayer.Core;
using OpenTK.Graphics.OpenGL;

namespace MiniAudioPlayer.Graphics
{
    public static class GraphicsContext
    {
        private static int screenWidth;
        private static int screenHeight;
        private static bool screenResized;
        private static ImGuiController imGuiController;

        internal static void Initialize(Window window)
        {
            screenWidth = window.FramebufferSize.X;
            screenHeight =  window.FramebufferSize.Y;
            screenResized = false;

            imGuiController = new ImGuiController(window);

            GL.Viewport(0, 0, screenWidth, screenHeight);
        }

        internal static void Destroy()
        {
            imGuiController.Dispose();
        }

        internal static void NewFrame()
        {
            InvalidateFrameBuffers();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        }

        internal static void BeginGUI()
        {
            imGuiController.NewFrame();
        }

        internal static void EndGUI()
        {
            imGuiController.EndFrame();
        }

        internal static void SetViewport(int x, int y, int width, int height)
        {
            if(width != screenWidth || height != screenHeight)
            {
                screenWidth = width;
                screenHeight = height;
                screenResized = true;
            }
        }

        private static void InvalidateFrameBuffers()
        {
            if(!screenResized)
                return;

            GL.Viewport(0, 0, screenWidth, screenHeight);

            screenResized = false;
        }

        public static int GetScreenWidth()
        {
            return screenWidth;
        }

        public static int GetScreenHeight()
        {
            return screenHeight;
        }
    }
}