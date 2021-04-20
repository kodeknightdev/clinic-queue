using System.Security.Cryptography;

namespace ClinicQueue.Web.Services
{
    /// <summary>
    /// Short random codes patients use to look up their place in the queue.
    /// Leaves out characters that are easy to confuse (0/O, 1/I/L).
    /// </summary>
    public static class LookupCode
    {
        private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        public const int Length = 6;

        public static string Generate()
        {
            var chars = new char[Length];
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
            }

            return new string(chars);
        }

        public static string Normalize(string code)
        {
            return code?.Trim().ToUpperInvariant();
        }
    }
}
