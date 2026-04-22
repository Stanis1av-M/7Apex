using System.ComponentModel.DataAnnotations;

namespace Apex7.Models
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Введите ваше полное имя")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Имя должно быть от 3 до 100 символов")]
        // РЕГУЛЯРНОЕ ВЫРАЖЕНИЕ: Разрешены только буквы (RU/EN), пробелы и дефисы
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "В имени разрешены только буквы, пробелы или дефис")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Введите корректный Email")]
        public string Email { get; set; } = null!;
    }
}