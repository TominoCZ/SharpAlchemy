using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Alchemy
{
    internal static class FontRenderer
    {
        //NOT my own code
        private static readonly int GlyphsPerLine = 16;

        private static readonly int GlyphLineCount = 16;
        private static readonly int GlyphWidth = 11;
        private static readonly int GlyphHeight = 22;

        private static readonly int CharXSpacing = 11;

        // Used to offset rendering glyphs to bitmap
        private static readonly int AtlasOffsetX = -3;

        private static readonly int AtlassOffsetY = -1;
        private static readonly int FontSize = 14;
        private static readonly bool BitmapFont = false;
        private static readonly string FontName = "Consolas";

        private static int _textureWidth;
        private static int _textureHeight;

        private static Font _font;

        public static void Init()
        {
            GenerateFontImage();
        }

        public static void DrawTextWithShadow(float x, float y, string text)
        {
            float[] color = new float[4];
            GL.GetFloat(GetPName.CurrentColor, color);

            GL.Color3(0, 0, 0);
            GL.Translate(2, 2, 0);
            DrawText(x, y, text);
            GL.Translate(-2, -2, 0);

            GL.Color4(color[0], color[1], color[2], 1);
            DrawText(x, y, text);
        }

        private static void DrawText(float x, float y, string text)
        {
            var tex = TextureManager.GetOrRegister("font");

            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.Begin(PrimitiveType.Quads);

            float uStep = GlyphWidth / (float)_textureWidth;
            float vStep = GlyphHeight / (float)_textureHeight;

            for (int n = 0; n < text.Length; n++)
            {
                char idx = text[n];
                float u = idx % GlyphsPerLine * uStep;
                float v = idx / GlyphsPerLine * vStep;

                GL.TexCoord2(u, v);
                GL.Vertex2(x, y);
                GL.TexCoord2(u + uStep, v);
                GL.Vertex2(x + GlyphWidth, y);
                GL.TexCoord2(u + uStep, v + vStep);
                GL.Vertex2(x + GlyphWidth, y + GlyphHeight);
                GL.TexCoord2(u, v + vStep);
                GL.Vertex2(x, y + GlyphHeight);

                x += CharXSpacing;
            }

            GL.End();
        }

        public static void DrawTextCentered(float x, float y, string text)
        {
            var size = TextRenderer.MeasureText(text, _font);

            DrawTextWithShadow(x - size.Width / 2 + CharXSpacing / 2, y - size.Height / 2, text);
        }

        private static void GenerateFontImage()
        {
            int bitmapWidth = GlyphsPerLine * GlyphWidth;
            int bitmapHeight = GlyphLineCount * GlyphHeight;

            using (Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                _font = new Font(new FontFamily(FontName), FontSize, FontStyle.Bold);

                using (var g = Graphics.FromImage(bitmap))
                {
                    if (BitmapFont)
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                    }
                    else
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    }

                    for (int p = 0; p < GlyphLineCount; p++)
                    {
                        for (int n = 0; n < GlyphsPerLine; n++)
                        {
                            char c = (char)(n + p * GlyphsPerLine);
                            g.DrawString(c.ToString(), _font, Brushes.White,
                                n * GlyphWidth + AtlasOffsetX, p * GlyphHeight + AtlassOffsetY);
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