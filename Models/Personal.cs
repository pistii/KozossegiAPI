using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
            personals = new HashSet<user>();
        }

        [Key]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [StringLength(30)]
        public string? firstName { get; set; }

        [StringLength(30)]
        public string? middleName { get; set; }

        [StringLength(30)]
        public string? lastName { get; set; }
        [StringLength(70)]
        public string? PlaceOfResidence { get; set; }

        public int? friendshipID { get; set; }

        public int? relationshipID { get; set; }

        public string? avatar { get; set; }

        [Column(TypeName = "int(16)")]
        public int? phoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? PlaceOfBirth { get; set; }

        public int? notificationId { get; set; }

        [JsonIgnore]
        public virtual ICollection<user> personals { get; set; }

        [InverseProperty("friendships")]
        [JsonIgnore]
        public virtual ICollection<Friendship>? Friends { get; set; } = new HashSet<Friendship>();

        [JsonIgnore]
        [InverseProperty("relationship")]
        public virtual ICollection<Relationship>? Relationships { get; set; } = new HashSet<Relationship>();

        [JsonIgnore]
        [InverseProperty("notification")]
        public virtual ICollection<Notification>? Notifications { get; set; } = new HashSet<Notification>();
    }

    public partial class Image
    {
        public string ImageName { get; set; }
        public string ImgType { get; set; }
    }
}