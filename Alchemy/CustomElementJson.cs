namespace Alchemy
{
    internal class CustomElementJson
    {
        public ElementNode[] Elements;

        public ElementCombinationNode[] ElementCombinations;

        public CustomElementJson(ElementNode[] elements, ElementCombinationNode[] elementCombinations)
        {
            Elements = elements;
            ElementCombinations = elementCombinations;
        }
    }
}