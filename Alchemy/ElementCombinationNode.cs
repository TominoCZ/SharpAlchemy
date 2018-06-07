using Newtonsoft.Json;

namespace Alchemy
{
    internal class ElementCombinationNode
    {
        [JsonProperty]
        public string Element1;

        [JsonProperty]
        public string Element2;

        [JsonProperty]
        public string[] Products;

        public ElementCombinationNode(string element1, string element2, params string[] products)
        {
            Element1 = element1;
            Element2 = element2;

            Products = products;
        }
    }
}