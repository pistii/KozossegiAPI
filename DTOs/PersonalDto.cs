using System.ComponentModel.DataAnnotations;

namespace KozossegiAPI.DTOs
{
    public class PersonalDto
    {
        public PersonalDto()
        {
            
        }

        public PersonalDto(int id, string firstName, string middleName, 
            string lastName, string placeOfResidence, string avatar, 
            DateOnly? dateOfBirth, string placeOfBirth, 
            string profession, string workplace)
        {
            this.id = id;
            this.firstName = firstName;
            this.middleName = middleName;
            this.lastName = lastName;
            this.PlaceOfResidence = placeOfResidence;
            this.avatar = avatar;
            this.DateOfBirth = dateOfBirth;
            this.PlaceOfBirth = placeOfBirth;
            this.Profession = profession;
            this.Workplace = workplace;
        }
        public int id { get; set; }

        [StringLength(30)]
        [Required(ErrorMessage = "First name is required")]
        public string? firstName { get; set; }

        [StringLength(30)]
        public string? middleName { get; set; }

        [StringLength(30)]
        [Required(ErrorMessage = "Last name is required")]
        public string? lastName { get; set; }

        [StringLength(70)]
        public string? PlaceOfResidence { get; set; }
        [StringLength(150)]
        public string? avatar { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? PlaceOfBirth { get; set; }

        [StringLength(60)]
        public string? Profession { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Workplace { get; set; } = string.Empty;
    }
}
