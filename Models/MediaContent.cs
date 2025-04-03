using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozossegiAPI.Models
{
    [Table("MediaContent")]
    public class MediaContent
    {
        public MediaContent()
        {
            
        }
        public MediaContent(int postId, string name, string type, long FileSize)
        {
            this.FK_PostId = postId;
            this.FileName = name;
            this.MediaType = type;
            this.FileSize = FileSize;
        }
        [Key]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int FK_PostId { get; set; }
        [StringLength(100)]
        public string? FileName { get; set; }
        [StringLength(80)]
        public string MediaType { get; set; }
        public long FileSize { get; set; }
        [JsonIgnore]
        public virtual Post Post{ get; set; }
    }

    public enum ContentType {
        Image = 0,
        Video = 1
    }
}
