using KozoskodoAPI.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozossegiAPI.Models
{
    public class ChatFile
    {
        [Key]
        public int FileId { get; set; }
        public int ChatContentId { get; set; }
        [StringLength(30)]
        public string FileType { get; set; }
        [StringLength(100)]
        public string FileToken { get; set; }
        [JsonIgnore]
        public virtual ChatContent ChatContent { get; set; }
    }
}
