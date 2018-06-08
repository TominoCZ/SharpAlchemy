using OpenTK.Graphics.OpenGL;
using System;

namespace Alchemy
{
    internal static class VertexUtil
    {
        public static void PutQuad(bool centered = true)
        {
            if (centered)
            {
                GL.TexCoord2(0, 0);
                GL.Vertex2(-0.5, -0.5);
                GL.TexCoord2(0, 1);
                GL.Vertex2(-0.5, 0.5);
                GL.TexCoord2(1, 1);
                GL.Vertex2(0.5, 0.5);
                GL.TexCoord2(1, 0);
                GL.Vertex2(0.5, -0.5);

                return;
            }

            GL.TexCoord2(0, 0);
            GL.Vertex2(0, 0);
            GL.TexCoord2(0, 1);
            GL.Vertex2(0, 1);
            GL.TexCoord2(1, 1);
            GL.Vertex2(1, 1);
            GL.TexCoord2(1, 0);
            GL.Vertex2(1, 0);
        }

        public static void PutCircle()
        {
            for (int i = 0; i < 60; i++)
            {
                var angle = i / 60f * 360 * Math.PI / 180;

                var x = Math.Cos(angle) / 2;
                var y = Math.Sin(angle) / 2;

                GL.Vertex2(x, y);
            }
        }
    }
}