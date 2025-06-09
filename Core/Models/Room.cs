namespace DormitoryManagement.Core.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public int Capacity { get; set; } = 6; // Default capacity
        public int BlockId { get; set; }

        // Navigation property for display purposes
        public string BlockName { get; set; } = string.Empty;
    }
}