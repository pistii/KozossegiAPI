namespace KozoskodoAPI.DTOs
{
    public class SignupDTO
    {
        public SignupDTO(string first_name, string middle_name, string last_name, string email, string password, string birthday) { FirstName = first_name;
            MiddleName = middle_name;
            LastName = last_name;
            Email = email;
            Password = password;
            Birthday = birthday;
        }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Birthday { get; set;
        }
    }
}
