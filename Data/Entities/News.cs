using System.ComponentModel.DataAnnotations;

namespace Apex7.Data.Entities
{
    public class News
    {
        [Key]
        public int NewsID { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageURL { get; set; } 
        public string? ImageURL2 { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsArchived { get; set; } = false;
    }
}
