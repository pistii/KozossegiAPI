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
        public Personal()
        {
            Personals = new HashSet<user>();
        }

        [Key]
        [Column(TypeName = "int(11)")]
        public int personalID { get; set; }

        [StringLength(100)]
        public string birthPlace { get; set; } = null!;

        [StringLength(30)]
        public string birthDay { get; set; } = null!;

        [StringLength(70)]
        public string email { get; set; } = null!;

        [StringLength(40)]
        public string password { get; set; } = null!;

        [StringLength(30)]
        public string registrationDate { get; set; } = null!;

        [JsonIgnore]
        [InverseProperty("personal")]
        public ICollection<user> Personals { get; set; }
    }
}