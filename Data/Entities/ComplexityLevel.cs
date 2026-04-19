namespace Apex7.Data.Entities
{
    public class ComplexityLevel
    {
        public int ComplexityLevelId { get; set; } // PK
        public string Name { get; set; } = null!;
        public string? ColorCode { get; set; } // Например, #FF0000

        public ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}
