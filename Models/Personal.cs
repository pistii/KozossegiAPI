using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace KozossegiAPI.Models
{

    /// <summary>
    /// personal infos about the user
    /// </summary>
    [Table("personal")]
    public partial class Personal
    {
        public Personal()
        {
            //users = new HashSet<user>();
        }

        [ForeignKey("users")]
        public int id { get; set; }
        
        [StringLength(30)]
        [Required(ErrorMessage = "First name is required")]
        public string firstName { get; set; }

        [StringLength(30)]
        public string? middleName { get; set; }
        
        [StringLength(30)]
        [Required(ErrorMessage = "Last name is required")]
        public string lastName { get; set; }
        public bool isMale { get; set; }
        [StringLength(70)]
        public string? PlaceOfResidence { get; set; }
        [StringLength(150)]
        public string? avatar { get; set; } = string.Empty;

        [StringLength(15)]
        public string? phoneNumber { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? PlaceOfBirth { get; set; }

        [StringLength(60)]
        public string? Profession { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Workplace { get; set; } = string.Empty;

        [ForeignKey("activeStudy")]
        public int? publicStudyId { get; set; }
        public virtual Study? activeStudy { get; set; }

        [JsonIgnore]
        public virtual Friend? friends { get; set; }
        
        [JsonIgnore]
        public virtual user? users { get; set; }

        [JsonIgnore]
        [InverseProperty("relationship")]
        public virtual ICollection<Relationship>? Relationships { get; set; } = new HashSet<Relationship>();

        [JsonIgnore]
        //[InverseProperty("notification")]
        public virtual ICollection<UserNotification>? UserNotification { get; set; } = new HashSet<UserNotification>();

        [JsonIgnore]
        public virtual ICollection<PersonalChatRoom> PersonalChatRooms { get; set; } = new HashSet<PersonalChatRoom>();

        [JsonIgnore]
        public virtual ICollection<PersonalPost> PersonalPosts { get; set; } = new HashSet<PersonalPost>();
        [JsonIgnore]
        public virtual Settings? Settings { get; set; } = new();

        //[JsonIgnore]
        //[InverseProperty("GetPersonals")]
        //public virtual ICollection<Friend>? Friends { get; set; } = new HashSet<Friend>();
    }

    /// <summary>
    /// Erre a táblára azért van szükség mert a user táblában kezelem az online státuszt, és az egyébként is kevésbé szükséges adatok elérését, de nincs szükség mindig az user táblát átadni. 
    /// </summary>
    public partial class Personal_IsOnlineDto
    {
        public Personal_IsOnlineDto(Personal user, bool isOnlineEnabled)
        {
            id = user.id;
            firstName = user.firstName;
            middleName = user.middleName;
            lastName = user.lastName;
            avatar = user.avatar;
            isOnline = isOnlineEnabled;
        }
        public int id { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string avatar { get; set; }
        public bool isOnline { get; set; } = false;
    }
}