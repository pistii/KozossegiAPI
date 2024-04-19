namespace KozossegiAPI.Services
{
    public class HelperService
    {
        public HelperService()
        {
            
        }

        public string GetFullname(string first, string? mid, string last)
        {
            if (string.IsNullOrEmpty(mid))
                return $"{first} {last}";
            return $"{first} {mid} {last}";
        }

    }
}
