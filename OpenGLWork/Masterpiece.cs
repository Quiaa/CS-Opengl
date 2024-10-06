using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Graphics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;

namespace OpenGLWork
{
    public class Masterpiece : GameWindow
    {
        private float[] vertices;
        private int vao;
        private int vbo;
        int shaderProgram;

        private int width, height;
        public Masterpiece(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;
            Title = "Masterpiece";
            CenterWindow(new Vector2i(width, height));
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            this.width = e.Width;
            this.height = e.Height;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            vertices = new float[]
            {
                // Base of the house (two triangles)
                -0.5f, -0.5f,   // Triangle 1: bottom-left
                 0.5f, -0.5f,   // Triangle 1: bottom-right
                 0.5f,  0.0f,   // Triangle 1: top-right

                -0.5f, -0.5f,   // Triangle 2: bottom-left
                 0.5f,  0.0f,   // Triangle 2: top-right
                -0.5f,  0.0f,   // Triangle 2: top-left

                // Roof of the house (a triangle)
                -0.6f,  0.0f,   // Left end of roof
                 0.6f,  0.0f,   // Right end of roof
                 0.0f,  0.5f    // Top of the roof
            };

            // generate the vbo
            vao = GL.GenVertexArray();

            // generate a buffer
            vbo = GL.GenBuffer();
            // bind the buffer as an array buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            // Store data in the vbo
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // bind the vao
            GL.BindVertexArray(vao);
            // point slot (0) of the VAO to the currently bound VBO (vbo)
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            // enable the slot
            GL.EnableVertexArrayAttrib(vao, 0);

            // unbind the vbo and vao respectively
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // create the shader program
            shaderProgram = GL.CreateProgram();

            // create the vertex shader
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            // add the source code from "Default.vert" in the Shaders file
            GL.ShaderSource(vertexShader, LoadShaderSource("Default.vert"));
            // Compile the Shader
            GL.CompileShader(vertexShader);

            // Same as vertex shader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, LoadShaderSource("Default.frag"));
            GL.CompileShader(fragmentShader);

            // Attach the shaders to the shader program
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            // Link the program to OpenGL
            GL.LinkProgram(shaderProgram);

            // delete the shaders
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            // Delete, VAO, VBO, Shader Program
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
            GL.DeleteProgram(shaderProgram);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // Set the color to fill the screen with
            GL.ClearColor(0.53f, 0.81f, 0.92f, 1.0f);
            // Fill the screen with the color
            GL.Clear(ClearBufferMask.ColorBufferBit);


            // draw our triangle
            GL.UseProgram(shaderProgram); // bind vao
            GL.BindVertexArray(vao); // use shader program
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6); 
            GL.DrawArrays(PrimitiveType.Triangles, 6, 3);

            // swap the buffers
            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        public static string LoadShaderSource(string filePath)
        {
            string shaderSource = "";

            try
            {
                using (StreamReader reader = new StreamReader("../../../Shaders/" + filePath))
                {
                    shaderSource = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load shader source file: " + e.Message);
            }

            return shaderSource;
        }
    }
}
