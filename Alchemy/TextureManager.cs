using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Alchemy
{
    internal static class TextureManager
    {
        private static readonly Dictionary<string, int> _textures = new Dictionary<string, int>();

        public static int GetOrRegister(string textureName, Bitmap bmp = null)
        {
            if (_textures.TryGetValue(textureName, out var texID))
                return texID;

            Bitmap img = bmp;

            if (img == null)
            {
                var file = "assets\\textures\\" + textureName + ".png";

                if (!File.Exists(file))
                {
                    Console.WriteLine($"Could not find file {file}");
                    return -1;
                }

                using (var fs = File.OpenRead(file))
                {
                    img = (Bitmap)Image.FromStream(fs);
                }
            }

            var ID = LoadTexture(img);

            _textures.Add(textureName, ID);

            return ID;
        }

        private static int LoadTexture(Bitmap img)
        {
            var ID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, ID);

            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            img.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);

            return ID;
        }
    }
}