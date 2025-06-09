namespace DormitoryManagement.Core.Models
{
    public class Dormitory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int TotalCapacity { get; set; }
        public int? DormOfficialId { get; set; }
    }
}