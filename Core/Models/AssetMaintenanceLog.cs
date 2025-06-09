namespace DormitoryManagement.Core.Models
{
    public class AssetMaintenanceLog
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string RequestDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Requested"; // Default status
    }
}