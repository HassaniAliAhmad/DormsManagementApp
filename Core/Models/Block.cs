using System.Collections.Generic;

namespace DormitoryManagement.Core.Models
{
    public class Block
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DormId { get; set; }
        public int? BlockOfficialId { get; set; }

        public string DormitoryName { get; set; } = string.Empty;
    }
}