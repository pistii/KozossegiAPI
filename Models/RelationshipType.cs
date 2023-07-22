using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KozoskodoAPI.Models
{

    public partial class RelationshipType
    {
        public RelationshipType() 
        {
            Relationshiptp = new HashSet<Relationship>();
        }

        [Key]
        [Column(TypeName = "int(11)")]
        public int relationshipTypeID { get; set; }

        [StringLength(40)]
        public string relationshipTitle { get; set; } = null!;


        [JsonIgnore]
        [InverseProperty("RelationshipTp")]
        public ICollection<Relationship> Relationshiptp { get; set; }
    }
}