using System.ComponentModel.DataAnnotations;

namespace Apex7.Data.Entities
{
    public class News
    {
        [Key]
        public int NewsID { get; set; }

        [Required(ErrorMessage = "Заголовок — это обязательное поле")]
        [StringLength(80, MinimumLength = 5, ErrorMessage = "Заголовок должен быть от 5 до 150 символов")]
        // Запрещаем спецсимволы, которые часто используются в SQL-инъекциях
        [RegularExpression(@"^[^'""\\;]*$", ErrorMessage = "Заголовок содержит недопустимые символы")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Описание экспедиции не может быть пустым")]
        [MaxLength(2500, ErrorMessage = "Описание слишком длинное (максимум 5000 символов)")]
        public string Content { get; set; } = null!;

        public string? ImageURL { get; set; }
        public string? ImageURL2 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsArchived { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}