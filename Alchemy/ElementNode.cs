using Newtonsoft.Json;

namespace Alchemy
{
    internal class ElementNode
    {
        [JsonProperty]
        public string Name;

        [JsonProperty]
        public string TextureName;

        [JsonProperty]
        public bool IsBaseElement;

        public ElementNode(string name, string textureName, bool isBaseElement = false)
        {
            Name = name;
            TextureName = textureName;

            IsBaseElement = isBaseElement;
        }
    }
}