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

using System;
using System.Runtime.InteropServices;
using GLFWNet;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MiniAudioPlayer.Graphics
{
    public static class OpenGL
    {
        public static readonly int Major;
        public static readonly int Minor;
        public static readonly float MaxAnisotropy;

        public static readonly int GL_TEXTURE_MAX_ANISOTROPY_EXT = 0x84FE;
        public static readonly int GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT = 0x84FF;
    
        public static unsafe void Initialize()
        {
            GLLoader.LoadBindings(new GLFWBindingsContext());

            string version = GL.GetString(StringName.Version);

            if(!string.IsNullOrEmpty(version))
                Console.WriteLine("OpenGL Version: " + version);

            fixed(int *pMajor = &Major)
            {
                GL.GetIntegerv(GetPName.MajorVersion, pMajor);
            }
            fixed(int *pMinor = &Minor)
            {
                GL.GetIntegerv(GetPName.MinorVersion, pMinor);
            }

            fixed(float *pMaxAnisotropy = &MaxAnisotropy)
            {
                GL.GetFloatv((GetPName)GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, pMaxAnisotropy);
            }

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Multisample);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
    }

    internal sealed class GLFWBindingsContext : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return Marshal.GetFunctionPointerForDelegate(GLFW.GetProcAddress(procName));
        }
    }
}