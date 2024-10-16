import pygame as pg
from OpenGL.GL import *
import numpy as np
from OpenGL.GL.shaders import compileProgram, compileShader
import random

class App:

    def __init__(self):        
        # Initialize pygame and OpenGL
        pg.init()
        pg.display.set_mode((800, 600), pg.OPENGL | pg.DOUBLEBUF)
        pg.display.set_caption("Cohen Sutherland")
        self.clock = pg.time.Clock()

        # Set up OpenGL settings
        glClearColor(0, 0, 0, 1)
        self.shader = self.create_shader("shaders/vertex.txt", "shaders/fragment.txt")
        glUseProgram(self.shader)

        # Create triangles and lines
        self.triangles = [
        #    Triangle((-0.5, -0.5), (0, -0.5), (-0.5, 0)),
        #    Triangle((0.5, 0.5), (0, 0.5), (0.5, 0))
        ]
        self.lines = [
        Line((-0.5, 0.5), (0.5, 0.5), (0, 0, 1)),  # top corner of the monitor
        Line((-0.5, -0.5), (0.5, -0.5), (0, 0, 1)),  # bottom corner of the monitor
        Line((-0.5, 0.5), (-0.5, -0.5), (0, 0, 1)),  # left corner of the monitor
        Line((0.5, 0.5), (0.5, -0.5), (0, 0, 1)),  # right corner of the monitor
        ] + [Line(self.random_point(), self.random_point(), self.random_color()) for _ in range(10)]  # random lines

        self.main_loop()

    def random_point(self):
        return (random.uniform(-1, 1), random.uniform(-1, 1))  # Random points potentially outside the screen
    def random_color(self):
        return (random.random(), random.random(), random.random())  # Generate random RGB colors
    
    def create_shader(self, vertex_file_path, fragment_file_path):
        def read_shader(file_path):
            with open(file_path, 'r') as file:
                return file.read()

        vertex_src = read_shader(vertex_file_path)
        fragment_src = read_shader(fragment_file_path)
        return compileProgram(
            compileShader(vertex_src, GL_VERTEX_SHADER),
            compileShader(fragment_src, GL_FRAGMENT_SHADER)
        )

    def main_loop(self):
        running = True
        while running:
            # Check for events
            for event in pg.event.get():
                if event.type == pg.QUIT:
                    running = False

            # Refresh screen
            glClear(GL_COLOR_BUFFER_BIT)
            glUseProgram(self.shader)

            # Draw triangles
            for triangle in self.triangles:
                triangle.draw()
            # Draw lines
            for line in self.lines:
                clipped_line = line.cohen_sutherland_clip(-0.5, 0.5, -0.5, 0.5)
                #clipped_line = line.liang_barsky_clip(-0.5, 0.5, -0.5, 0.5)
                if clipped_line:
                    clipped_line.draw()

            pg.display.flip()
            self.clock.tick(60)

        self.quit()

    def quit(self):
        for triangle in self.triangles:
            triangle.destroy()
        for lines in self.lines:
            lines.destroy()
        glDeleteProgram(self.shader)
        pg.quit()


class Triangle:

    def __init__(self, p1, p2, p3):
        vertices = np.array([
            *p1, 0.0, 1.0, 0.0, 0.0,
            *p2, 0.0, 0.0, 1.0, 0.0,
            *p3, 0.0, 0.0, 0.0, 1.0
        ], dtype=np.float32)

        self.vertex_count = 3
        self.vao = glGenVertexArrays(1)
        self.vbo = glGenBuffers(1)

        # Bind and set buffer data
        glBindVertexArray(self.vao)
        glBindBuffer(GL_ARRAY_BUFFER, self.vbo)
        glBufferData(GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL_STATIC_DRAW)

        # Vertex positions
        glEnableVertexAttribArray(0)
        glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 24, ctypes.c_void_p(0))
        # Vertex colors
        glEnableVertexAttribArray(1)
        glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 24, ctypes.c_void_p(12))

    def draw(self):
        glBindVertexArray(self.vao)
        glDrawArrays(GL_TRIANGLES, 0, self.vertex_count)

    def destroy(self):
        glDeleteVertexArrays(1, (self.vao,))
        glDeleteBuffers(1, (self.vbo,))

class Line:

    def __init__(self, p1, p2, color):
        self.p1 = p1
        self.p2 = p2
        self.color = color
        # Line vertices with colors
        vertices = np.array([
            *p1, 0.0, *color, 
            *p2, 0.0, *color 
        ], dtype=np.float32)

        self.vertex_count = 2
        self.vao = glGenVertexArrays(1)
        self.vbo = glGenBuffers(1)

        # Bind and set buffer data
        glBindVertexArray(self.vao)
        glBindBuffer(GL_ARRAY_BUFFER, self.vbo)
        glBufferData(GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL_STATIC_DRAW)

        # Vertex positions
        glEnableVertexAttribArray(0)
        glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 24, ctypes.c_void_p(0))
        # Vertex colors
        glEnableVertexAttribArray(1)
        glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 24, ctypes.c_void_p(12))

    def cohen_sutherland_clip(self, x_min, x_max, y_min, y_max):
        def compute_code(x, y):
            code = 0
            if x < x_min: code |= 1  # Left
            if x > x_max: code |= 2  # Right
            if y < y_min: code |= 4  # Bottom
            if y > y_max: code |= 8  # Top
            return code

        x1, y1 = self.p1
        x2, y2 = self.p2
        code1 = compute_code(x1, y1)
        code2 = compute_code(x2, y2)

        while True:
            if not (code1 | code2):
                return Line((x1, y1), (x2, y2), self.color)  # Inside
            elif code1 & code2:
                return None  # Outside
            else:
                x, y = 0, 0
                outcode = code1 if code1 else code2

                if outcode & 8:  # Top
                    x = x1 + (x2 - x1) * (y_max - y1) / (y2 - y1)
                    y = y_max
                elif outcode & 4:  # Bottom
                    x = x1 + (x2 - x1) * (y_min - y1) / (y2 - y1)
                    y = y_min
                elif outcode & 2:  # Right
                    y = y1 + (y2 - y1) * (x_max - x1) / (x2 - x1)
                    x = x_max
                elif outcode & 1:  # Left
                    y = y1 + (y2 - y1) * (x_min - x1) / (x2 - x1)
                    x = x_min

                if outcode == code1:
                    x1, y1 = x, y
                    code1 = compute_code(x1, y1)
                else:
                    x2, y2 = x, y
                    code2 = compute_code(x2, y2)

    def liang_barsky_clip(self, x_min, x_max, y_min, y_max):
        x1, y1 = self.p1
        x2, y2 = self.p2
        dx = x2 - x1
        dy = y2 - y1

        p = [-dx, dx, -dy, dy]
        q = [x1 - x_min, x_max - x1, y1 - y_min, y_max - y1]
        u1, u2 = 0.0, 1.0

        for i in range(4):
            if p[i] == 0 and q[i] < 0:
                return None
            if p[i] != 0:
                t = q[i] / p[i]
                if p[i] < 0:
                    u1 = max(u1, t)
                else:
                    u2 = min(u2, t)

        if u1 > u2:
            return None

        nx1, ny1 = x1 + u1 * dx, y1 + u1 * dy
        nx2, ny2 = x1 + u2 * dx, y1 + u2 * dy
        return Line((nx1, ny1), (nx2, ny2), self.color)
    
    def draw(self):
        glBindVertexArray(self.vao)
        glDrawArrays(GL_LINES, 0, self.vertex_count)

    def destroy(self):
        glDeleteVertexArrays(1, (self.vao,))
        glDeleteBuffers(1, (self.vbo,))

if __name__ == "__main__":
    App()
