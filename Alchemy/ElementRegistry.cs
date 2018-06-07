using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Alchemy
{
    public static class ElementRegistry
    {
        private static readonly ConcurrentDictionary<Tuple<Element, Element>, Element[]> _combinations = new ConcurrentDictionary<Tuple<Element, Element>, Element[]>();

        private static readonly List<Element> _allElements = new List<Element>();

        public static void RegisterCombination(Element e1, Element e2, params Element[] products)
        {
            if (CombinationExists(e1, e2))
                throw new DuplicateCombinationException();

            var combination = new Tuple<Element, Element>(e1, e2);

            if (!_allElements.Contains(e1))
                _allElements.Add(e1);
            if (!_allElements.Contains(e2))
                _allElements.Add(e2);

            _combinations.TryAdd(combination, products);
        }

        public static Element[] GetProducts(Element e1, Element e2)
        {
            foreach (var pair in _combinations)
            {
                var combination = pair.Key;

                if (combination.Item1 == e1 && combination.Item2 == e2 ||
                    combination.Item2 == e1 && combination.Item1 == e2)
                    return pair.Value;
            }

            return new Element[0];
        }

        private static bool CombinationExists(Element e1, Element e2)
        {
            foreach (var pair in _combinations)
            {
                var combination = pair.Key;

                if (combination.Item1 == e1 && combination.Item2 == e2 ||
                    combination.Item1 == e2 && combination.Item2 == e1)
                    return true;
            }

            return false;
        }
    }
}