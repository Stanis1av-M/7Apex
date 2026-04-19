using System.ComponentModel.DataAnnotations;

namespace Apex7.Models // ПРОВЕРЬ ТУТ
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите полное имя")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}