using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ProjectManager.Validators
{
    public class StringValidator
    {
        public static bool ContainsOnlyLettersAndDigits(string input)
        {
            // Regex pattern: Allows English letters (a-z, A-Z), Arabic letters (\u0600-\u06FF), and digits (0-9)
            return Regex.IsMatch(input, @"^[a-zA-Z\u0600-\u06FF0-9\s]+$");
        }

        public static bool ContainsAtLeastOneLetter(string input)
        {
            // Regex pattern: Must contain at least one English or Arabic letter
            return Regex.IsMatch(input, @"[a-zA-Z\u0600-\u06FF]");
        }
        public static bool IsValidEmail(string email)
        {
            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
