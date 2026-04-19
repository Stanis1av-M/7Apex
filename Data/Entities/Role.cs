namespace Apex7.Data.Entities
{
    public class Role
    {
        public int RoleId { get; set; } // PK
        public string Name { get; set; } = null!; // Админ, Менеджер, Гид, Клиент

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
