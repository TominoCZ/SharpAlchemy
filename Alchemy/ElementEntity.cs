using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace Alchemy
{
    internal class ElementEntity
    {
        public Element Element;

        public float X, Y;

        public static int ElementIconSize = 64;

        private int _ticks;
        private int _ticksLast;

        private readonly int _maxTicks = 3;

        public ElementEntity(float x, float y, Element element)
        {
            X = x;
            Y = y;

            Element = element;
        }

        public void Update()
        {
            _ticksLast = _ticks;

            if (_ticks < _maxTicks)
                _ticks++;
        }

        public void Render(float partialTicks)
        {
            var partialTick = _ticksLast + (_ticks - _ticksLast) * partialTicks;
            var progress = Math.Min(partialTick, _maxTicks) / _maxTicks;

            GL.Translate(X, Y, 0);
            GL.Scale(ElementIconSize, ElementIconSize, 1);

            GL.BindTexture(TextureTarget.Texture2D, Element.TextureId);

            GL.Begin(PrimitiveType.Quads);
            GL.Color4(1f, 1, 1, progress);
            VertexUtil.PutQuad();
            GL.End();

            GL.Scale(1f / ElementIconSize, 1f / ElementIconSize, 1);
            GL.Translate(-X, -Y, 0);

            FontRenderer.DrawTextCentered(X, Y + ElementIconSize / 1.5f + 5, Element.ToString());
        }

        public bool IsMouseOver(PointF p)
        {
            var x = p.X - X;
            var y = p.Y - Y;

            var dist = Math.Sqrt(x * x + y * y);

            return dist <= ElementIconSize;
        }
    }
}