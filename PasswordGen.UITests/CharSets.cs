namespace PasswordGen.UITests
{
    using System.Linq;
    using System.Text;

    internal static class Charsets
    {
        internal static readonly string _digits;
        internal static readonly string _lowercase;
        internal static readonly string _uppercase;
        internal static readonly string _symbols;
        internal static readonly string _ascii;

        static Charsets()
        {
            _ascii = BuildCharset('!', '~');
            _digits = BuildCharset('0', '9');
            _lowercase = BuildCharset('a', 'z');
            _uppercase = BuildCharset('A', 'Z');
            _symbols = string.Join(null, _ascii.Except(_digits).Except(_lowercase).Except(_uppercase));
        }

        /// <summary>
        /// Build a character set from the first and last members of a contiguous character sequence.
        /// </summary>
        private static string BuildCharset(char first, char last)
        {
            var charset = new StringBuilder(last - first + 1);
            while (first <= last)
                charset.Append(first++);
            return charset.ToString();
        }
    }
}
