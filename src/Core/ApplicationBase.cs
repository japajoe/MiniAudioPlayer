using System;
using MiniAudioPlayer.Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace MiniAudioPlayer.Core
{
    public abstract class ApplicationBase : IDisposable
    {
        protected Window window;

        public ApplicationBase()
        {
            GameWindowSettings gameWindowSettings = GameWindowSettings.Default;

            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "OpenTK 5",
                Flags = ContextFlags.Debug,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3),
                Vsync = VSyncMode.On
            };

            window = new Window(gameWindowSettings, nativeWindowSettings);
            SetCallbacks();
        }

        public ApplicationBase(int width, int height, int glMajor, int glMinor, bool vsync, string title)
        {
            GameWindowSettings gameWindowSettings = GameWindowSettings.Default;

            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(width, height),
                Title = title,
                Flags = ContextFlags.Debug,
                Profile = ContextProfile.Core,
                APIVersion = new Version(glMajor, glMinor),
                Vsync = vsync ? VSyncMode.On : VSyncMode.Off
            };

            window = new Window(gameWindowSettings, nativeWindowSettings);
            SetCallbacks();
        }

        private void SetCallbacks()
        {
            window.Initialize += () => {
                GraphicsContext.Initialize(window);
                OnInitialize();
            };

            window.Unload += () => {
                GraphicsContext.Destroy();
                OnDestroy();
            };

            window.NewFrame += () => {
                Time.NewFrame();
                OnNewFrame();
            };

            window.Render += () => {
                GraphicsContext.NewFrame();
                GraphicsContext.BeginGUI();
                OnGUI();
                GraphicsContext.EndGUI();                
            };

            window.Resize += (ResizeEventArgs args) => {
                GraphicsContext.SetViewport(0, 0, args.Width, args.Height);
                OnResize(args);
            };
        }

        public void Run()
        {
            window.Run();
        }

        protected virtual void OnInitialize()
        {
            
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnNewFrame()
        {
        }

        protected virtual void OnRenderFrame()
        {

        }

        protected virtual void OnGUI()
        {
            
        }

        protected virtual void OnResize(ResizeEventArgs args)
        {
            
        }

        public void Dispose()
        {
            window.Dispose();
        }
    }
}