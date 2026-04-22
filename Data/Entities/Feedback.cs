using System.ComponentModel.DataAnnotations; // Обязательно для работы валидации

namespace Apex7.Data.Entities
{
    public enum FeedbackStatus { Pending, Accepted, Rejected }

    public class Feedback
    {
        public int FeedbackID { get; set; }

        // Связываем с пользователем, чтобы он видел решение
        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите ваше имя")]
        [StringLength(50, ErrorMessage = "Имя не может быть длиннее 50 символов")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Пожалуйста, укажите ваш Email")]
        [EmailAddress(ErrorMessage = "Введите корректный Email (например, user@mail.ru)")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Сообщение не может быть пустым")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Сообщение должно содержать минимум 10 символов")]
        public string Message { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Новые поля для решения админа
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
        [StringLength(300, ErrorMessage = "Ответ администратора не может превышать 300 символов")]
        public string? AdminResponse { get; set; }
        
    }
}