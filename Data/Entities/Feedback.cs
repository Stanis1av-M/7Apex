namespace Apex7.Data.Entities
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsProcessed { get; set; } = false; 
    }
}
