namespace DormitoryManagement.Core.Models
{
    public class Student : Person
    {
        public string StudentId { get; set; } = string.Empty;
        public int? RoomId { get; set; }

        
        public string FullName => $"{FirstName} {LastName}";

        public string? DormitoryName { get; set; }
        public string? BlockName { get; set; }
        public string? RoomNumber { get; set; }
    }
}