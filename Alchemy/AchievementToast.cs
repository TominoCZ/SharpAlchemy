using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Alchemy
{
    internal class AchievementToast
    {
        public string Title;
        public string MessageText;

        private readonly int _iconTextureId;
        private readonly int _toastTextureId;

        private readonly Game _game;

        private long _ticks;
        private long _ticksLast;

        private readonly long _maxTicks = 60;

        public bool IsDead;

        public AchievementToast(string title, string messageText, int iconId, Game game)
        {
            Title = title;
            MessageText = messageText;

            _game = game;

            _iconTextureId = iconId;
            _toastTextureId = TextureManager.GetOrRegister("toast");
        }

        public void Update()
        {
            _ticksLast = _ticks;

            if (_ticks < _maxTicks)
                _ticks++;
            else
                IsDead = true;
        }

        public void Render(float partialTicks)
        {
            if (IsDead)
                return;

            float progress;

            var partialTick = _ticksLast + (_ticks - _ticksLast) * partialTicks;

            if (partialTick >= _maxTicks - 5)
                progress = (float)Math.Cos(Math.Min(5 - _maxTicks - partialTick, 5) / 5f * MathHelper.PiOver2);
            else
                progress = -(float)Math.Sin(Math.Min(partialTick, 10) / 10f * MathHelper.PiOver2);

            var centerX = _game.Width / 2;
            var centerY = -32 - 64 * progress;

            GL.Translate(centerX, centerY, 0);

            //render toast texture
            GL.BindTexture(TextureTarget.Texture2D, _toastTextureId);

            GL.Color4(1f, 1, 1, 1);
            GL.Scale(256, 64, 1);
            GL.Begin(PrimitiveType.Quads);
            VertexUtil.PutQuad();
            GL.End();
            GL.Scale(1f / 256, 1f / 64, 1);

            //render icon shadow
            GL.BindTexture(TextureTarget.Texture2D, _iconTextureId);

            GL.Color4(0f, 0, 0, 1);
            GL.Translate(-94.5F, 2, 0);
            GL.Scale(49, 49, 1);
            GL.Begin(PrimitiveType.Quads);
            VertexUtil.PutQuad();
            GL.End();
            GL.Scale(1f / 49, 1f / 49, 1);
            GL.Translate(94.5F, -2, 0);

            //render icon
            GL.Color4(1f, 1, 1, 1);
            GL.Translate(-96.5F, 0, 0);
            GL.Scale(49, 49, 1);
            GL.Begin(PrimitiveType.Quads);
            VertexUtil.PutQuad();
            GL.End();
            GL.Scale(1f / 49, 1f / 49, 1);
            GL.Translate(96.5F, 0, 0);

            //render title
            GL.Color4(0, 0.65f, 1, 1);
            GL.Scale(0.9f, 0.9f, 1);
            FontRenderer.DrawTextWithShadow(-67, -28, Title);
            GL.Scale(1 / 0.9f, 1 / 0.9f, 1);

            //render text
            GL.Color4(1f, 1, 1, 1);
            GL.Scale(0.9f, 0.9f, 1);
            FontRenderer.DrawTextWithShadow(-67, -4, MessageText);
            GL.Scale(1 / 0.9f, 1 / 0.9f, 1);

            GL.Translate(-centerX, -centerY, 0);
        }
    }
}