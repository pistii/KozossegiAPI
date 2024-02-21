using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    public class BlockedPerson
    {
        [Column(TypeName = "int(11)")]
        public int BlockedPersonId { get; set; }
    }
}
