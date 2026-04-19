using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Apex7.Data.Entities
{
    public class Tour
    {
        [Key]
        public int TourId { get; set; }

        public int ComplexityLevelId { get; set; }
        public ComplexityLevel ComplexityLevel { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Region { get; set; }
        public string? Description { get; set; }
        public int DurationDays { get; set; }

       
        public string? ImageUrl { get; set; }

        public ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();
    }
}