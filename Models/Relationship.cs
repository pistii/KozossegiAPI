using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
{
    [MetadataType(typeof(user))]
    public partial class Relationship
    {
        public Relationship()
        {
            Relationships = new HashSet<user>();
        }

        [Key]
        [Column(TypeName = "int(11)")]
        public int? relationshipID { get; set; }

        [Column(TypeName = "int(11)")]
        public int? typeID { get; set; }

        [JsonIgnore]
        [InverseProperty("relationship")]
        public ICollection<user> Relationships { get; set; }


        [ForeignKey("typeID")]
        [InverseProperty("Relationshiptp")]
        [JsonIgnore]
        public virtual RelationshipType? RelationshipTp { get; set; }
    }
}