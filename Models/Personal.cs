using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KozoskodoAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
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
        public string? firstName { get; set; }

        [StringLength(30)]
        public string? middleName { get; set; }

        [StringLength(30)]
        public string? lastName { get; set; }
        public bool isMale { get; set; }
        [StringLength(70)]
        public string? PlaceOfResidence { get; set; }
        [StringLength(150)]
        public string? avatar { get; set; } = string.Empty;

        [Column(TypeName = "int(16)")]
        public int? phoneNumber { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? PlaceOfBirth { get; set; }

        
        [JsonIgnore]
        public virtual Friend? friends { get; set; }
        
        [JsonIgnore]
        public virtual user? users { get; set; }

        [JsonIgnore]
        [InverseProperty("relationship")]
        public virtual ICollection<Relationship>? Relationships { get; set; } = new HashSet<Relationship>();

        [JsonIgnore]
        [InverseProperty("notification")]
        public virtual ICollection<Notification>? Notifications { get; set; } = new HashSet<Notification>();

        [JsonIgnore]
        public virtual ICollection<PersonalChatRoom> PersonalChatRooms { get; set; } = new HashSet<PersonalChatRoom>();

        [JsonIgnore]
        public virtual ICollection<PersonalPost> PersonalPosts { get; set; } = new HashSet<PersonalPost>();

    }

    public partial class Personal_IsOnlineDto : Personal
    {
        public bool isOnline { get; set; } = false;
    }
}