using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Alchemy
{
    public static class ElementRegistry
    {
        private static readonly ConcurrentDictionary<Tuple<Element, Element>, Element[]> Combinations = new ConcurrentDictionary<Tuple<Element, Element>, Element[]>();

        private static readonly List<Element> AllElements = new List<Element>();

        public static void RegisterElement(Element e)
        {
            if (!AllElements.Contains(e))
                AllElements.Add(e);
        }

        public static void RegisterCombination(Element e1, Element e2, params Element[] products)
        {
            if (CombinationExists(e1, e2))
                throw new DuplicateCombinationException();

            var combination = new Tuple<Element, Element>(e1, e2);

            if (!AllElements.Contains(e1))
                AllElements.Add(e1);
            if (!AllElements.Contains(e2))
                AllElements.Add(e2);

            Combinations.TryAdd(combination, products);
        }

        public static int GetTotalCount()
        {
            return AllElements.Count;
        }

        public static Element[] GetBaseElements()
        {
            return AllElements.Where(e => e.IsBaseElement).ToArray();
        }

        public static Element[] GetProducts(Element e1, Element e2)
        {
            foreach (var pair in Combinations)
            {
                var combination = pair.Key;

                if (combination.Item1 == e1 && combination.Item2 == e2 ||
                    combination.Item2 == e1 && combination.Item1 == e2)
                    return pair.Value;
            }

            return new Element[0];
        }

        public static Element GetElement(string name)
        {
            return AllElements.SingleOrDefault(element => element.ToString() == name);
        }

        private static bool CombinationExists(Element e1, Element e2)
        {
            foreach (var pair in Combinations)
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