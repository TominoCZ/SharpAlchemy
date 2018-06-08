using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Alchemy
{
    class InventoryGui
    {
        private List<Element> _elementEntities = new List<Element>();

        private readonly Game _game;

        private bool _shown;

        private readonly float _size = 256;
        private readonly float _iconGap = 5f;
        private readonly float _iconSize = 64f;

        private int _ticks, _ticksLast;
        private readonly int _ticksMax = 4;

        private Point _lastMouse;

        private Element _lastOver;

        public bool MouseOverTrigger { get; private set; }

        public bool MouseOverRectangle { get; private set; }

        public InventoryGui(Game game)
        {
            _game = game;
        }

        public void Update()
        {
            _elementEntities = _game.GetLearntElements();

            _ticksLast = _ticks;

            if (_shown)
            {
                if (_ticks < _ticksMax)
                    _ticks++;
            }
            else
            {
                if (_ticks > 0)
                    _ticks--;
            }
        }

        public void Render(float partialTicks)
        {
            var partialAngle = Math.Min(_ticksMax, _ticksLast + (_ticks - _ticksLast) * partialTicks) / _ticksMax * MathHelper.PiOver2;

            if (partialAngle <= 0.1)
                return;

            var x = _game.Width - (float)Math.Sin(partialAngle) * _size;

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Translate(x, 32, 0);
            GL.Scale(_size, _game.Height, 1);

            //render background
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(0, 0.75, 1, 0.15f);
            VertexUtil.PutQuad(false);
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(0, 0.75, 1, 0.5f);
            VertexUtil.PutQuad(false);
            GL.End();
            GL.Scale(1 / _size, 1f / _game.Height, 1);

            //render elements
            var count = (int)(_size / _iconSize);
            _lastOver = null;

            for (int i = 0; i < _elementEntities.Count; i++)
            {
                var e = _elementEntities[i];

                var x1 = i % count * _iconSize;
                var y1 = i / count * _iconSize;

                if (IsPointInRectangle(x + x1, y1 + 32, _iconSize, _iconSize, _lastMouse.X, _lastMouse.Y))
                    _lastOver = e;

                GL.BindTexture(TextureTarget.Texture2D, e.TextureId);

                var newSize = _iconSize - _iconGap * 2;

                GL.Translate(x1 + _iconSize / 2, y1 + _iconSize / 2, 0);
                GL.Scale(newSize, newSize, 1);

                GL.Translate(2 / newSize, 2 / newSize, 0);
                GL.Color3(0, 0, 0);
                GL.Begin(PrimitiveType.Quads);
                VertexUtil.PutQuad();
                GL.End();
                GL.Translate(-2 / newSize, -2 / newSize, 0);

                GL.Color3(1, 1, 1f);
                GL.Begin(PrimitiveType.Quads);
                VertexUtil.PutQuad();
                GL.End();

                GL.Scale(1f / newSize, 1f / newSize, 1);
                GL.Translate(-x1 - _iconSize / 2, -y1 - _iconSize / 2, 0);
            }

            FontRenderer.DrawTextCentered(128, -16, "DISCOVERED");

            GL.Translate(-x, -32, 0);
        }

        public Element PickElement(int x, int y)
        {
            return _lastOver;
        }

        public void MouseMove(int x, int y)
        {
            _lastMouse = new Point(x, y);

            var triggerWidth = 16;

            var rectX = _game.Width - triggerWidth;
            var rectW = triggerWidth;
            var rectH = _game.Height;

            MouseOverTrigger = IsPointInRectangle(rectX, 0, rectW, rectH, x, y); //mouse over the trigger area
            MouseOverRectangle = IsPointInRectangle(_game.Width - _size, 0, _size, rectH, x, y);
        }

        public void SetShown(bool shown)
        {
            _shown = shown;
        }

        public bool IsShown()
        {
            return _shown;
        }

        private bool IsPointInRectangle(float rectX, float rectY, float rectW, float rectH, float x, float y)
        {
            if (rectX <= x && x < rectX + rectW && rectY <= y)
                return y < rectY + rectH;

            return false;
        }
    }
}