using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MiniAudioPlayer
{
    public sealed class Shader
    {
        private int id;

        public int Id
        {
            get
            {
                return id;
            }
        }

        public Shader()
        {
            id = 0;
        }

        public bool Generate(string vertexSource, string fragmentSource, out string error)
        {
            int vertShader = Compile(vertexSource, ShaderType.VertexShader, out error);

            if(vertShader == 0)
                return false;

            int fragShader = Compile(fragmentSource, ShaderType.FragmentShader, out error);

            if(fragShader == 0)
                return false;

            bool success = false;

            if(vertShader > 0 && fragShader > 0)
            {
                id = GL.CreateProgram();
                
                GL.AttachShader(id, vertShader);
                GL.AttachShader(id, fragShader);
                GL.LinkProgram(id);

                bool failed = true;

                if(GL.GetProgrami(id, ProgramProperty.LinkStatus) == 0)
                {
                    GL.GetProgramInfoLog(id, out error);
                    Console.WriteLine(error);
                }
                else
                {
                    success = true;
                }

                GL.DeleteShader(vertShader);
                GL.DeleteShader(fragShader);

                return failed;
            }

            return success;
        }

        public void Use()
        {
            GL.UseProgram(id);
        }

        public void Delete()
        {
            if(id > 0)
            {
                GL.DeleteProgram(id);
                id = 0;
            }
        }

        public void SetFloat(string name, float value)
        {
            SetFloat(GL.GetUniformLocation(id, name), value);
        }

        public void SetFloat2(string name, Vector2 value)
        {
            SetFloat2(GL.GetUniformLocation(id, name), value.X, value.Y);
        }

        public void SetFloat2(string name, float x, float y)
        {
            SetFloat2(GL.GetUniformLocation(id, name), x, y);
        }

        public void SetFloat3(string name, Vector3 value)
        {
            SetFloat3(GL.GetUniformLocation(id, name), value);
        }

        public void SetFloat3(string name, Color value)
        {
            SetFloat3(GL.GetUniformLocation(id, name), value);
        }

        public void SetFloat3(string name, float x, float y, float z)
        {
            SetFloat3(GL.GetUniformLocation(id, name), x, y, z);
        }

        public void SetFloat4(string name, float x, float y, float z, float w)
        {
            SetFloat4(GL.GetUniformLocation(id, name), x, y, z, w);
        }

        public void SetFloat4(string name, Vector4 value)
        {
            SetFloat4(GL.GetUniformLocation(id, name), value);
        }

        public void SetFloat4(string name, Color value)
        {
            SetFloat4(GL.GetUniformLocation(id, name), value.r, value.g, value.b, value.a);
        }

        public void SetInt(string name, int value)
        {
            SetInt(GL.GetUniformLocation(id, name), value);
        }

        public void SetMat4(string name, Matrix4 value, bool transpose = false)
        {
            SetMat4(GL.GetUniformLocation(id, name), value, transpose);
        }

        public void SetMat3(string name, Matrix3 value, bool transpose = false)
        {
            SetMat3(GL.GetUniformLocation(id, name), value, transpose);
        }

        public void SetBool(string name, bool value)
        {
            int val = value == false ? 0 : 1;
            SetInt(GL.GetUniformLocation(id, name), val);
        }

        public void SetFloat(int location, float value)
        {
            GL.Uniform1f(location, value);
        }

        public void SetFloat2(int location, Vector2 value)
        {
            GL.Uniform2f(location, value.X, value.Y);
        }

        public void SetFloat2(int location, float x, float y)
        {
            GL.Uniform2f(location, x, y);
        }

        public void SetFloat3(int location, Vector3 value)
        {
            GL.Uniform3f(location, value.X, value.Y, value.Z);
        }

        public void SetFloat3(int location, Color value)
        {
            GL.Uniform3f(location, value.r, value.g, value.b);
        }

        public void SetFloat3(int location, float x, float y, float z)
        {
            GL.Uniform3f(location, x, y, z);
        }

        public void SetFloat4(int location, Vector4 value)
        {
            GL.Uniform4f(location, value.X, value.Y, value.Z, value.W);
        }

        public void SetFloat4(int location, Color value)
        {
            SetFloat4(location, value.r, value.g, value.b, value.a);
        }

        public void SetFloat4(int location, float x, float y, float z, float w)
        {
            GL.Uniform4f(location, x, y, z, w);
        }

        public void SetInt(int location, int value)
        {
            GL.Uniform1i(location, value);
        }

        public void SetMat4(int location, Matrix4 value, bool transpose = false)
        {
            GL.UniformMatrix4f(location, 1, transpose, value);
        }

        public void SetMat3(int location, Matrix3 value, bool transpose = false)
        {
            GL.UniformMatrix3f(location, 1, transpose, value);
        }

        public void SetBool(int location, bool value)
        {
            int val = value == false ? 0 : 1;
            GL.Uniform1i(location, val);
        }

        private static int Compile(string source, ShaderType type, out string error)
        {
            error = string.Empty;

            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            
            if(GL.GetShaderi(shader, ShaderParameterName.CompileStatus) == 0)
            {
                GL.GetShaderInfoLog(shader, out error);
                //Console.WriteLine(type.ToString() + " " + error);
                //Console.WriteLine("=====");
                //Console.WriteLine(source);
                //Console.WriteLine("=====");
                return 0;
            }
            return shader;
        }
    }
}