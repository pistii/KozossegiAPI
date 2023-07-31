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
        public user() {
            Personals = new HashSet<Personal>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int(11)")]
        public int userID { get; set; }


        [StringLength(70)]
        public string email { get; set; } = null!;

        [StringLength(40)]
        public string password { get; set; } = null!;

        [StringLength(30)]
        public string? registrationDate { get; set; }

        public int? personalID { get; set; }

        [InverseProperty("personal")]
        [ForeignKey("id")]
        public virtual ICollection<Personal>? Personals { get; }
    }
}