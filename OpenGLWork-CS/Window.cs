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
    public class Window : GameWindow
    {
        private float[] vertices;

        private int vao;
        private int vbo;
        int shaderProgram;
        
        private int width, height;
        public Window(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;
            Title = "LineDraw";
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

             // generate the vbo
             vao = GL.GenVertexArray();

             Vector2 start = new Vector2(-0.5f, -0.8f);
             Vector2 end = new Vector2(0.5f, 0.3f);
             vertices = GenerateLineVerticesDDA(start, end);
             //vertices = GenerateLineVerticesBresenham(start, end);

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
            GL.ClearColor(0.0f, 0.0f, 0f, 0f);
            // Fill the screen with the color
            GL.Clear(ClearBufferMask.ColorBufferBit);


            // draw our triangle
            GL.UseProgram(shaderProgram); // bind vao
            GL.BindVertexArray(vao); // use shader program
            GL.DrawArrays(PrimitiveType.LineStrip, 0, vertices.Length/2); // draw the triangle | args = Primitive type, first vertex, last vertex


            // swap the buffers
            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        private float[] GenerateLineVerticesDDA(Vector2 start, Vector2 end)
        {
            List<float> lineVertices = new List<float>();

            // DDA Algorithm
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;

            float steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

            float xIncrement = dx / steps;
            float yIncrement = dy / steps;

            float x = start.X;
            float y = start.Y;

            for (int i = 0; i <= steps; i++)
            {
                lineVertices.Add(x);
                lineVertices.Add(y);
                x += xIncrement;
                y += yIncrement;
            }

            return lineVertices.ToArray();
        }
        private float[] GenerateLineVerticesBresenham(Vector2 start, Vector2 end)
        {
            List<float> lineVertices = new List<float>();

            // Bresenham's Algorithm
            int x1 = (int)(start.X * 1000); // Scale to avoid floating point issues
            int y1 = (int)(start.Y * 1000);
            int x2 = (int)(end.X * 1000);
            int y2 = (int)(end.Y * 1000);

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);

            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;

            int err = dx - dy;

            while (true)
            {
                // Scale down to normalized device coordinates (NDC)
                lineVertices.Add(x1 / 1000f);
                lineVertices.Add(y1 / 1000f);

                if (x1 == x2 && y1 == y2)
                    break;

                int e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }

            return lineVertices.ToArray();
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
