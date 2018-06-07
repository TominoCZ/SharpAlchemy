namespace Alchemy
{
    public class Element
    {
        public static Element Fire;
        public static Element Water;

        private readonly string _displayName;
        public int TextureID { get; }

        public Element(string displayName, string textureName)
        {
            _displayName = displayName;
            TextureID = TextureManager.GetOrRegister(textureName);
        }

        public override string ToString()
        {
            return _displayName;
        }
    }
}