namespace Apex7.Data.Entities
{
    public class TourGroup
    {
        public int TourGroupId { get; set; } // PK

        public int TourId { get; set; } // FK
        public Tour Tour { get; set; } = null!;

        public int GuideId { get; set; } // FK на User (Гид)
        public User Guide { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxCapacity { get; set; }

        public ICollection<TourBooking> Bookings { get; set; } = new List<TourBooking>();
    }
}
