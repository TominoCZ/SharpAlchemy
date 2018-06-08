using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Alchemy
{
    class TrashEntity
    {
        private readonly Game _game;

        private readonly float _size = 128;
        private readonly float _offsetX = -8;
        private readonly float _offsetY = 8;

        private float _currentY;

        private int _ticks, _ticksLast;
        private readonly int _ticksMax = 8;
        
        private bool _shown;

        public bool MouseOver { get; private set; }
        public bool MouseOverDestination { get; private set; }

        public TrashEntity(Game game)
        {
            _game = game;
        }

        public void Update()
        {
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

            var offX = _size + _offsetX;
            var offY = _size + _offsetY;

            var x = _game.Width - offX;

            _currentY = _game.Height - (float)Math.Sin(partialAngle) * offY;

            var tex = TextureManager.GetOrRegister(MouseOver && _shown ? "trash_open" : "trash_closed");

            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.Translate(x, _currentY, 0);
            GL.Scale(_size, _size, 1);

            GL.Begin(PrimitiveType.Quads);

            GL.Color3(1, 1, 1f);
            VertexUtil.PutQuad(false);
            GL.End();
            GL.Scale(1 / _size, 1 / _size, 1);
            GL.Translate(-x, -_currentY, 0);
        }

        public void SetShown(bool shown)
        {
            _shown = shown;
        }

        public void MouseMove(int x, int y)
        {
            var point = new Point(x, y);
            var rect = new Rectangle((int)(_game.Width - (_size + _offsetX)), (int)_currentY, (int)_size, (int)_size);

            MouseOver = rect.Contains(point);

            rect.Y = (int) (_game.Height - (_size + _offsetY));

            MouseOverDestination = rect.Contains(point);
        }
    }
}