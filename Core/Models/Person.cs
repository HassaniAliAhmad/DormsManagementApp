namespace DormitoryManagement.Core.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        // --- NEW PROPERTY ---
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}