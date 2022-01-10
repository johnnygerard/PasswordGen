namespace PasswordGen.Utilities
{
    using System.Collections.Generic;

    internal class PasswordDataEntry
    {
        internal IEnumerable<char> CharSet { get; set; }
        internal int Length { get; set; }

        internal PasswordDataEntry(IEnumerable<char> charSet, int length)
        {
            CharSet = charSet;
            Length = length;
        }
    }
}
