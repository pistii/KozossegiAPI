using KozossegiAPI.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    public partial class Study
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_Id { get; set; }
        [JsonIgnore]
        public int FK_UserId { get; set; }

        [StringLength(120)]
        [Required(ErrorMessage = "School name must be given")]
        public string? SchoolName { get; set; }
        [StringLength(120)]
        public string? Class { get; set; }
        [Range(1900, 2099, ErrorMessage = "Invalid date time.")]
        public int? StartYear { get; set; }
        [Range(1900, 2099, ErrorMessage = "Invalid date time.")]
        public int? EndYear { get; set; }

        public long initId { get; set; }

        public Study(int FK_UserId, string? SchoolName, string? Class, int? StartYear, int? EndYear)
        {
            this.FK_UserId = FK_UserId;
            this.SchoolName = SchoolName;
            this.Class = Class;
            this.StartYear = StartYear;
            this.EndYear = EndYear;
        }

        public Study()
        {
            
        }
        [JsonIgnore]
        public virtual user? user { get; }
    }
}
