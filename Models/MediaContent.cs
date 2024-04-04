using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    [Table("MediaContent")]
    public class MediaContent
    {
        public MediaContent()
        {
            
        }
        public MediaContent(int mediaContentId, string name, ContentType type)
        {
            this.MediaContentId = mediaContentId;
            this.FileName = name;
            this.ContentType = type;
        }
        [Key]
        [Column(TypeName = "int(11)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int MediaContentId { get; set; }
        [StringLength(100)]
        public string? FileName { get; set; }
        public ContentType ContentType { get; set; }
        [JsonIgnore]
        public Post Post { get; set; }
    }

    public enum ContentType {
        Image = 0,
        Video = 1
    }
}
