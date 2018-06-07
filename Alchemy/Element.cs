namespace Alchemy
{
    public class Element
    {
        private readonly string _displayName;
        public int TextureId { get; }
        public bool IsBaseElement { get; }

        public Element(string displayName, string textureName, bool isBaseElement = false)
        {
            _displayName = displayName;

            TextureId = TextureManager.GetOrRegister(textureName);

            IsBaseElement = isBaseElement;
        }

        public override string ToString()
        {
            return _displayName;
        }
    }
}