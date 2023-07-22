using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
{

    public partial class Friendship
    {
        public Friendship()
        {
            Friendships = new HashSet<user>();
        }
        [Key]
        [Column(TypeName = "int(11)")]
        public int? friendshipID { get; set; }

        [Column(TypeName = "int(11)")]
        public int? friends { get; set; }

        [Column(TypeName = "int(11)")]
        public int blockedFriend { get; set; }

        [Column(TypeName = "int(11)")]
        public int followedPerson { get; set; }


        [JsonIgnore]
        [InverseProperty("friendship")]
        public ICollection<user> Friendships { get; set; }
    }
}