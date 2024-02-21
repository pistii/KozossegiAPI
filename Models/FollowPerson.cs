using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    public class FollowPerson
    {

        [Column(TypeName = "int(11)")]
        public int FollowedPersonId { get; set; }

    }
}
