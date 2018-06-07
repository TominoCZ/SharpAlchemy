using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace Alchemy
{
    internal static class FontRenderer
    {
        //NOT my own code
        private static readonly int _glyphsPerLine = 16;

        private static readonly int _glyphLineCount = 16;
        private static readonly int _glyphWidth = 11;
        private static readonly int _glyphHeight = 22;

        private static readonly int _charXSpacing = 11;

        // Used to offset rendering glyphs to bitmap
        private static readonly int _atlasOffsetX = -3;

        private static readonly int _atlassOffsetY = -1;
        private static readonly int _fontSize = 14;
        private static readonly bool _bitmapFont = false;
        private static readonly string _fontName = "Consolas";

        private static int _textureWidth;
        private static int _textureHeight;

        private static Font _font;

        public static void Init()
        {
            GenerateFontImage();
        }

        public static void DrawText(float x, float y, string text)
        {
            var tex = TextureManager.GetOrRegister("font");

            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.Begin(PrimitiveType.Quads);

            float u_step = _glyphWidth / (float)_textureWidth;
            float v_step = _glyphHeight / (float)_textureHeight;

            for (int n = 0; n < text.Length; n++)
            {
                char idx = text[n];
                float u = (float)(idx % _glyphsPerLine) * u_step;
                float v = (float)(idx / _glyphsPerLine) * v_step;

                GL.TexCoord2(u, v);
                GL.Vertex2(x, y);
                GL.TexCoord2(u + u_step, v);
                GL.Vertex2(x + _glyphWidth, y);
                GL.TexCoord2(u + u_step, v + v_step);
                GL.Vertex2(x + _glyphWidth, y + _glyphHeight);
                GL.TexCoord2(u, v + v_step);
                GL.Vertex2(x, y + _glyphHeight);

                x += _charXSpacing;
            }

            GL.End();
        }

        public static void DrawTextCentered(float x, float y, string text)
        {
            var size = TextRenderer.MeasureText(text, _font);

            DrawText(x - size.Width / 2 + _charXSpacing / 2, y - size.Height / 2, text);
        }

        private static void GenerateFontImage()
        {
            int bitmapWidth = _glyphsPerLine * _glyphWidth;
            int bitmapHeight = _glyphLineCount * _glyphHeight;

            using (Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                _font = new Font(new FontFamily(_fontName), _fontSize, FontStyle.Bold);

                using (var g = Graphics.FromImage(bitmap))
                {
                    if (_bitmapFont)
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                    }
                    else
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        //g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    }

                    for (int p = 0; p < _glyphLineCount; p++)
                    {
                        for (int n = 0; n < _glyphsPerLine; n++)
                        {
                            char c = (char)(n + p * _glyphsPerLine);
                            g.DrawString(c.ToString(), _font, Brushes.White,
                                n * _glyphWidth + _atlasOffsetX, p * _glyphHeight + _atlassOffsetY);
                        }
                    }
                }

                _textureWidth = bitmap.Width;
                _textureHeight = bitmapHeight;

                TextureManager.GetOrRegister("font", bitmap);
            }
        }
    }
}