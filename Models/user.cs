using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KozoskodoAPI.DTOs;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
{
   public partial class user
    {
        public user()
        {
            UserRestriction = new HashSet<UserRestriction>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int(11)")]
        public int userID { get; set; }

        [StringLength(30)]
        public string? email { get; set; }
        [StringLength(30)]
        [JsonIgnore]
        public string? SecondaryEmailAddress { get; set; }

        [StringLength(40)]
        [JsonIgnore]
        public string? password { get; set; }
        public DateTime? registrationDate { get; set; } = DateTime.Now;
        public bool isActivated { get; set; } = false;
        [JsonIgnore]
        public string? Guid { get; set; }
        public DateTime LastOnline { get; set; }
        public bool isOnlineEnabled { get; set; }
        public virtual ICollection<UserRestriction> UserRestriction { get; }
        public virtual Personal? personal { get; set; }
        public virtual ICollection<Studies>? Studies { get; set; }

    }
}