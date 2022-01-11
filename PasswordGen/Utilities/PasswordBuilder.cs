namespace PasswordGen.Utilities
{
    using System.Collections.Generic;
    using System.Linq;

    using static CryptoRNG;

    internal static class PasswordBuilder
    {

        /// <summary>
        /// Return a randomly generated password based on a list of charsets with associated length.
        /// </summary>
        internal static string BuildPassword(IEnumerable<PasswordDataEntry> passwordData, int passwordLength)
        {
            var password = new char[passwordLength];
            int passwordIndex = 0;

            foreach (var passwordDataEntry in passwordData)
            {
                IEnumerable<char> charset = passwordDataEntry.Charset;
                int charsetCount = charset.Count();

                for (int i = 0; i < passwordDataEntry.Length; i++)
                    password[passwordIndex++] = charset.ElementAt(GetRandomInt(charsetCount));
            }

            Shuffle(password);
            return new string(password);
        }


        /// <summary>
        /// Randomly reorder the characters in <paramref name="password"/>.
        /// </summary>
        /// <remarks>
        /// Algorithm used: Fisher–Yates shuffle
        /// </remarks>
        internal static void Shuffle(char[] password)
        {
            for (int i = password.Length - 1; i > 0; i--)
            {
                int randomIndex = GetRandomInt(i + 1);

                // Swap the last character with a random character in the range [0, i].
                char temp = password[i];
                password[i] = password[randomIndex];
                password[randomIndex] = temp;
            }
        }
    }
}
