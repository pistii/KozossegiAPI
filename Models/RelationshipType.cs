using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace KozossegiAPI.Models
{

    public partial class RelationshipType
    {
        public RelationshipType() 
        {
        }

        [Key]
        [Column(TypeName = "int(11)")]
        public int relationshipTypeID { get; set; }

        [StringLength(40)]
        public string relationshipTitle { get; set; } = null!;


        [ForeignKey("typeID")]
        [InverseProperty("RelationshipTypes")]
        [JsonIgnore]
        public virtual Relationship? RelationshipTp { get; set; }
    }
}