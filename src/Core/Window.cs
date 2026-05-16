using System;
using OpenTK.Graphics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiniAudioPlayer.Core
{
    public delegate void InitializeEvent();
    public delegate void DestroyEvent();
    public delegate void NewFrameEvent();
    public delegate void RenderEvent();

    public class Window : GameWindow
    {
        public event InitializeEvent Initialize;
        public event DestroyEvent Destroy;
        public event NewFrameEvent NewFrame;
        public event RenderEvent Render;

        private GLFWCallbacks.ErrorCallback errorCallback;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            errorCallback = OnGLFWError;
            GLFWProvider.SetErrorCallback(errorCallback);
            GLLoader.LoadBindings(new GLFWBindingsContext());
            Initialize?.Invoke();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            Destroy?.Invoke();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            NewFrame?.Invoke();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            Render?.Invoke();

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }

        private static void OnGLFWError(ErrorCode error, string description)
        {
            Console.WriteLine(description);
        }
    }
}