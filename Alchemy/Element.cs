namespace Alchemy
{
    public class Element
    {
        public static Element Fire;
        public static Element Water;
        public static Element Earth;
        public static Element Air;

        private readonly string _displayName;
        public int TextureId { get; }

        public Element(string displayName, string textureName)
        {
            _displayName = displayName;
            TextureId = TextureManager.GetOrRegister(textureName);
        }

        public override string ToString()
        {
            return _displayName;
        }
    }
}