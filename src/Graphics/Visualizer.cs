using System;
using System.Collections.Generic;
using MiniAudioPlayer.Core;
using MiniAudioPlayer.Embedded;
using MiniAudioPlayer.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MiniAudioPlayer
{
    public class TextureInfo
    {
        public Texture2D target;
        public int bindingIndex;
        public string uniformName;

        public TextureInfo(Texture2D target, int bindingIndex, string uniformName)
        {
            this.target = target;
            this.bindingIndex = bindingIndex;
            this.uniformName = uniformName;
        }
    }

    public sealed class Visualizer
    {
        private FrameBuffer[] frameBuffers;
        private PingPongFrameBuffer pingpongBuffer;
        private List<TextureInfo> textures;
        private int vao;
        private int width;
        private int height;
        private Shader shader;

        public int Texture
        {
            get => pingpongBuffer.dst.GetColorAttachment(0);
        }

        public Visualizer()
        {
            FrameBufferTextureSpecification colorAttachment = new FrameBufferTextureSpecification
            {
                format = FrameBufferTextureFormat.RGBA8,
                wrap = TextureWrapMode.ClampToEdge,
                filter = TextureFilterMode.Linear
            };

            width = 128;
            height = 128;

            FrameBufferSpecification fboSpec = new FrameBufferSpecification{
                width = width,
                height = height,
                samples = 1,
                resizable = true,
                attachments = {
                    colorAttachment
                }
            };

            frameBuffers = new FrameBuffer[2];

            for(int i = 0; i < frameBuffers.Length; i++)
            {
                frameBuffers[i] = new FrameBuffer();
                frameBuffers[i].Generate(fboSpec);    
            }

            pingpongBuffer = new PingPongFrameBuffer();
            pingpongBuffer.src = frameBuffers[0];
            pingpongBuffer.dst = frameBuffers[1];

            GL.GenVertexArrays(1, ref vao);

            string fragmentSource = BasicShader.HeaderSource + "\n#line 1\n" + BasicShader.FragmentSource;
            shader = new Shader();
            shader.Generate(BasicShader.VertexSource, fragmentSource, out string error);

            textures = new List<TextureInfo>();
        }

        public void SetShader(Shader shader)
        {
            if(this.shader.Id > 0)
                this.shader.Delete();
            this.shader = shader;
        }

        public void AddTexture(Texture2D target, string uniformName)
        {
            int bindingIndex = textures.Count + 1;
            textures.Add(new TextureInfo(target, bindingIndex, uniformName));
        }

        public void Render()
        {
            InvalidateFrameBuffers();

            pingpongBuffer.dst.Bind();
            pingpongBuffer.dst.Clear(new Color(0, 0, 0, 1));

            shader.Use();

            GL.ActiveTexture(TextureUnit.Texture0); 
            GL.BindTexture(TextureTarget.Texture2D, pingpongBuffer.src.GetColorAttachment(0));
            shader.SetInt("uTexture", 0);

            for(int i = 0; i < textures.Count; i++)
            {
                int bindingIndex = textures[i].bindingIndex;
                GL.ActiveTexture((TextureUnit)((int)TextureUnit.Texture0 + bindingIndex)); 
                textures[i].target.Bind();
                shader.SetInt(textures[i].uniformName, bindingIndex);
            }

            shader.SetFloat("uTime", Time.Elapsed);
            shader.SetFloat2("uResolution", new OpenTK.Mathematics.Vector2(pingpongBuffer.dst.GetWidth(), pingpongBuffer.dst.GetHeight()));

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.BindVertexArray(0);

            pingpongBuffer.dst.Unbind();

            GL.Viewport(0, 0, GraphicsContext.GetScreenWidth(), GraphicsContext.GetScreenHeight());

            var tmp = pingpongBuffer.src;
            pingpongBuffer.src = pingpongBuffer.dst;
            pingpongBuffer.dst = tmp;
        }

        public void SetSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        private void InvalidateFrameBuffers()
        {
            if(width < 10 || height < 10)
                return;

            if (width != frameBuffers[0].GetWidth() || height != frameBuffers[0].GetHeight())
            {
                frameBuffers[0].Resize(width, height);
                frameBuffers[0].Bind();
                frameBuffers[0].Clear(Color.Black);

                frameBuffers[1].Resize(width, height);
                frameBuffers[1].Bind();
                frameBuffers[1].Clear(Color.Black);
                frameBuffers[1].Unbind();
            }
        }
    }
}