namespace DormitoryManagement.Core.Models
{
    public class Asset
    {
        public int Id { get; set; }
        public string AssetNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // e.g., "Bed", "Fridge"
        public string PartNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // e.g., "Healthy", "Faulty"
        
        // Foreign Keys for assignment
        public int? RoomId { get; set; }
        public int? OwningStudentId { get; set; } 

        public string? RoomNumber { get; set; }
        public string? BlockName { get; set; }
        public string? DormitoryName { get; set; }
        public string? StudentFullName { get; set; }
    }
}