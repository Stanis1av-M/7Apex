namespace Apex7.Data.Entities
{
    public class TourBooking
    {
        public int TourBookingId { get; set; } // PK

        public int TourGroupId { get; set; } // FK
        public TourGroup TourGroup { get; set; } = null!;

        public int UserId { get; set; } // FK (Клиент)
        public User User { get; set; } = null!;

        public string? Status { get; set; } // Забронировано, Оплачено, Отменено
        public DateTime BookingDate { get; set; } = DateTime.Now;
    }
}
