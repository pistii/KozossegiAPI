using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KozossegiAPI.DTOs
{
    public class StudyDto
    {
        public int Id { get; set; }
        public long initId { get; set; }
        [JsonIgnore]
        public int FK_UserId { get; set; }
        [StringLength(120)]
        [Required(ErrorMessage = "School name must be given")]
        public string SchoolName { get; set; }
        [StringLength(120)]
        public string? Class { get; set; }
        [Range(1900, 2099, ErrorMessage = "Invalid date time.")]
        public int? StartYear { get; set; }
        [Range(1900, 2099, ErrorMessage = "Invalid date time.")]
        public int? EndYear { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
