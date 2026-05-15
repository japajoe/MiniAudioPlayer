using System;
using System.Collections.Specialized;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MiniAudioPlayer.Graphics
{
    public sealed class Texture2D
    {
        private int id;
        private int width;
        private int height;

        public int Id => id;
        public int Width => width;
        public int Height => height;

        public Texture2D(int width, int height, InternalFormat internalFormat, PixelFormat pixelFormat, PixelType pixelType, TextureSettings settings, bool generateMipmaps)
        {
            this.width = width;
            this.height = height;
            
            id = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2d, id);

            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)settings.wrapS);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)settings.wrapT);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)settings.minFilter);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)settings.magFilter);

            if(!generateMipmaps)
            {
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMaxLevel, 0);
            }

            GL.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, width, height, 0, pixelFormat, pixelType, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2d, 0);
        }

        public void Delete()
        {
            GL.DeleteTexture(id);
            id = 0;
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2d, id);
        }

        public void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2d, 0);
        }

        public void SubImage2D(int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, ReadOnlySpan<float> pixels)
        {
            GL.TexSubImage2D(TextureTarget.Texture2d, 0, xoffset, yoffset, width, height, format, type, pixels);
        }

        public void SubImage2D(int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, ReadOnlySpan<byte> pixels)
        {
            GL.TexSubImage2D(TextureTarget.Texture2d, 0, xoffset, yoffset, width, height, format, type, pixels);
        }

        public void SubImage2D(int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, ReadOnlySpan<Vector3> pixels)
        {
            GL.TexSubImage2D(TextureTarget.Texture2d, 0, xoffset, yoffset, width, height, format, type, pixels);
        }
    }
}