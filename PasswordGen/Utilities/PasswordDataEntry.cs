namespace PasswordGen.Utilities
{
    using System.Collections.Generic;

    internal class PasswordDataEntry
    {
        internal IEnumerable<char> Charset { get; set; }
        internal int Length { get; set; }

        internal PasswordDataEntry(IEnumerable<char> charset, int length)
        {
            Charset = charset;
            Length = length;
        }
    }
}
