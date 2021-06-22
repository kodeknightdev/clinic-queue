using System.Linq;

namespace ClinicQueue.Web.Services
{
    public static class PhoneNumber
    {
        /// <summary>
        /// Strips formatting so "(555) 010-2030" and "555 010 2030" compare equal.
        /// Keeps a leading + for international numbers. Returns null for blank input.
        /// </summary>
        public static string Normalize(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            var trimmed = phone.Trim();
            var digits = new string(trimmed.Where(char.IsDigit).ToArray());
            if (digits.Length == 0)
            {
                return null;
            }

            return trimmed.StartsWith("+") ? "+" + digits : digits;
        }
    }
}
