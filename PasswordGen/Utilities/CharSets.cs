namespace PasswordGen.Utilities
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    internal static class Charsets
    {
        /// <summary>
        /// Main charset key
        /// </summary>
        internal const string MAIN_CHARSET = nameof(MAIN_CHARSET);

        // charset keys
        internal const string DIGITS = nameof(DIGITS);
        internal const string SYMBOLS = nameof(SYMBOLS);
        internal const string LOWERCASE = nameof(LOWERCASE);
        internal const string UPPERCASE = nameof(UPPERCASE);

        /// <summary>
        /// ASCII charset without C0, SPACE and DEL characters.
        /// </summary>
        internal static readonly string _ascii;
        internal static readonly ReadOnlyDictionary<string, string> _fullCharsets;
        internal static readonly string[] _charsetKeys = new string[]
        {
            DIGITS,
            SYMBOLS,
            LOWERCASE,
            UPPERCASE,
        };

        static Charsets()
        {
            _ascii = BuildCharset('!', '~');
            string digits = BuildCharset('0', '9');
            string lowercase = BuildCharset('a', 'z');
            string uppercase = BuildCharset('A', 'Z');

            IEnumerable<char> symbols = _ascii
                .Except(digits)
                .Except(lowercase)
                .Except(uppercase);

            _fullCharsets = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { DIGITS, digits},
                { SYMBOLS, string.Join(null, symbols) },
                { LOWERCASE, lowercase},
                { UPPERCASE, uppercase},
            });
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
