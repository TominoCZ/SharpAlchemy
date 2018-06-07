using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Alchemy
{
    internal class ElementEntity
    {
        public Element Element;

        public float X, Y;

        public static int ElementIconSize = 64;

        public ElementEntity(float x, float y, Element element)
        {
            X = x;
            Y = y;

            Element = element;
        }

        public void Render()
        {
            GL.Translate(X, Y, 0);
            GL.Scale(ElementIconSize, ElementIconSize, 1);

            GL.BindTexture(TextureTarget.Texture2D, Element.TextureID);

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(1f, 1, 1);
            VertexUtil.PutQuad();
            GL.End();

            GL.Scale(1f / ElementIconSize, 1f / ElementIconSize, 1);
            GL.Translate(-X, -Y, 0);

            FontRenderer.DrawTextCentered(X, Y + ElementIconSize / 1.5f, Element.ToString());
        }

        public bool IsMouseOver(PointF p)
        {
            var x = p.X - X;
            var y = p.Y - Y;

            var dist = Math.Sqrt(x * x + y * y);

            return dist <= ElementIconSize / 2f;
        }
    }
}