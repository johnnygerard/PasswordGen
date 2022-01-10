namespace PasswordGen.Utilities
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    internal static class CharSets
    {
        internal const string DIGITS = nameof(DIGITS);
        internal const string LOWERCASE = nameof(LOWERCASE);
        internal const string UPPERCASE = nameof(UPPERCASE);
        internal const string SYMBOLS = nameof(SYMBOLS);
        internal const string ASCII = nameof(ASCII);

        internal static readonly ReadOnlyDictionary<string, string> _fullCharSets;

        static CharSets()
        {
            string _ascii = BuildCharSet('!', '~');
            string digits = BuildCharSet('0', '9');
            string lowercase = BuildCharSet('a', 'z');
            string uppercase = BuildCharSet('A', 'Z');

            IEnumerable<char> symbols = _ascii
                .Except(digits)
                .Except(lowercase)
                .Except(uppercase);

            _fullCharSets = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { DIGITS, digits},
                { LOWERCASE, lowercase},
                { UPPERCASE, uppercase},
                { SYMBOLS, string.Join(null, symbols) },
                { ASCII, _ascii },
            });
        }

        /// <summary>
        /// Build a character set from the first and last members of a contiguous character sequence.
        /// </summary>
        private static string BuildCharSet(char first, char last)
        {
            var charSet = new StringBuilder(last - first + 1);
            while (first <= last)
                charSet.Append(first++);
            return charSet.ToString();
        }
    }
}
