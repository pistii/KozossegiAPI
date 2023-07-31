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
    public partial class Personal
    {
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int? id { get; set; }
        [StringLength(30)]
        public string firstName { get; set; } = null!;

        [StringLength(30)]
        public string? middleName { get; set; }

        [StringLength(30)]
        public string lastName { get; set; } = null!;

        public int? friendshipID { get; set; }

        public int? relationshipID { get; set; }

        [StringLength(120)]
        public string? avatar { get; set; }

        [Column(TypeName = "int(16)")]
        public int phoneNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        [StringLength(100)]
        public string? BirthOfPlace { get; set; }


        [InverseProperty("Personals")]
        [JsonIgnore]

        public virtual user? personal { get; set; }

        [InverseProperty("friendships")]
        [JsonIgnore]
        public virtual ICollection<Friendship>? Friends { get; } = new HashSet<Friendship>();

        [JsonIgnore]
        [InverseProperty("relationship")]

        public virtual ICollection<Relationship>? Relationships { get; set; } = new HashSet<Relationship>();
    }
}