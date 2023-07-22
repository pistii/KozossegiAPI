using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
{
    public partial class user
    {
        [Key]
        [Column(TypeName = "int(11)")]
        public int userID { get; set; }

        [StringLength(30)]
        public string firstName { get; set; } = null!;

        [StringLength(30)]
        public string? middleName { get; set; }

        [StringLength(30)]
        public string lastName { get; set; } = null!;

        [Column(TypeName = "int(11)")]
        public int? friendshipID { get; set; }

        [Column(TypeName = "int(11)")]
        public int? relationshipID { get; set; }

        [StringLength(120)]
        public string avatar { get; set; } = null!;

        [Column(TypeName = "int(16)")]
        public int phoneNumber { get; set; }

        [Column(TypeName = "int(11)")]
        public int? personalID { get; set; }


        [ForeignKey("friendshipID")]
        [InverseProperty("Friendships")]
        [JsonIgnore]
        public virtual Friendship? friendship { get; set; }
        [ForeignKey("relationshipID")]
        [InverseProperty("Relationships")]
        [JsonIgnore]
        public virtual Relationship? relationship { get; set; }
        [ForeignKey("personalID")]
        [InverseProperty("Personals")]
        [JsonIgnore]
        public virtual Personal? personal { get; set; }
    }
}