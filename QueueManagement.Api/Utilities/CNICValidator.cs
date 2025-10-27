using System.Text.RegularExpressions;

namespace QueueManagement.Api.Utilities
{
    public static class CNICValidator
    {
        public static bool IsValid(string cnic)
        {
            if (string.IsNullOrEmpty(cnic) || cnic.Length != 13)
                return false;

            // Check if all characters are digits
            if (!Regex.IsMatch(cnic, @"^\d{13}$"))
                return false;

            return true;
        }
    }
}