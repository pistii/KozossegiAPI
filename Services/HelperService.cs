using System.ComponentModel;
using System.Reflection;

namespace KozossegiAPI.Services
{
    public static class HelperService
    {

        public static string GetFullname(string first, string? mid, string last)
        {
            if (string.IsNullOrEmpty(mid))
                return $"{first} {last}";
            return $"{first} {mid} {last}";
        }

        public static byte[] ConvertToByteArray(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static bool PasswordIsValid(string password)
        {
            int minCapitalLetters = 1; //Determines minimum how many capital letters must contain
            return password.Length > 8 && 
                (password.Where(char.IsUpper).Count() >= minCapitalLetters);
        }


        public static string GetEnumDescription(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute != null ? attribute.Description : value.ToString();
        }
    }
}
