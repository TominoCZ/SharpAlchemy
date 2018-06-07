using System;

namespace Alchemy
{
    internal class DuplicateCombinationException : Exception
    {
        public DuplicateCombinationException() : base("Attempted to register an already existing combination")
        {
        }
    }
}