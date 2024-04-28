using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KozoskodoAPI.Models
{
    public partial class Studies
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_Id { get; set; }
        public int FK_UserId { get; set; }
        [StringLength(120)]
        public string? SchoolName { get; set; }
        [StringLength(120)]
        public string? Class { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public virtual user user { get; }

        public Studies(int FK_UserId, string? SchoolName, string? Class, int? StartYear, int? EndYear)
        {
            this.FK_UserId = FK_UserId;
            this.SchoolName = SchoolName;
            this.Class = Class;
            this.StartYear = StartYear;
            this.EndYear = EndYear;
        }

        public Studies()
        {
            
        }
    }
}
