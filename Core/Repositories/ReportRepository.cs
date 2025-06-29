using DormitoryManagement.Core.Database;
using DormitoryManagement.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace DormitoryManagement.Core.Repositories
{
    public class ReportRepository
    {
        private DbConnection _dbConnection;

        public ReportRepository()
        {
            _dbConnection = new DbConnection();
        }

        // Report 1: Get general accommodation statistics
        public Dictionary<string, string> GetAccommodationStats()
        {
            var stats = new Dictionary<string, string>();
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Total Dormitories
                command.CommandText = "SELECT COUNT(*) FROM Dormitories";
                stats["Total Dormitories"] = command.ExecuteScalar().ToString();

                // Total Blocks
                command.CommandText = "SELECT COUNT(*) FROM Blocks";
                stats["Total Blocks"] = command.ExecuteScalar().ToString();
                
                // Total Rooms
                command.CommandText = "SELECT COUNT(*) FROM Rooms";
                var totalRooms = (long)command.ExecuteScalar();
                stats["Total Rooms"] = totalRooms.ToString();

                // Total Students
                command.CommandText = "SELECT COUNT(*) FROM Students";
                stats["Total Students Registered"] = command.ExecuteScalar().ToString();
                
                // Students with rooms
                command.CommandText = "SELECT COUNT(*) FROM Students WHERE RoomId IS NOT NULL";
                var housedStudents = (long)command.ExecuteScalar();
                stats["Students Housed"] = housedStudents.ToString();
                
                // Total Capacity
                command.CommandText = "SELECT SUM(Capacity) FROM Rooms";
                var totalCapacity = command.ExecuteScalar();
                stats["Total Room Capacity"] = totalCapacity is System.DBNull ? "0" : totalCapacity.ToString();
                
                // Empty Rooms (rooms with no students assigned)
                command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM Rooms r 
                    LEFT JOIN Students s ON r.Id = s.RoomId 
                    GROUP BY r.Id 
                    HAVING COUNT(s.PersonId) = 0";
                // This query is a bit tricky, a simpler way is to count rooms not in the students table
                command.CommandText = "SELECT COUNT(*) FROM Rooms WHERE Id NOT IN (SELECT DISTINCT RoomId FROM Students WHERE RoomId IS NOT NULL)";
                stats["Empty Rooms"] = command.ExecuteScalar().ToString();
            }
            return stats;
        }

        // Report 2: Get a list of rooms and their occupancy count
        public List<object> GetRoomOccupancy()
        {
            var occupancyList = new List<object>();
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        d.Name as Dormitory,
                        b.Name as Block,
                        r.RoomNumber,
                        r.Capacity,
                        COUNT(s.PersonId) as Occupants
                    FROM Rooms r
                    JOIN Blocks b ON r.BlockId = b.Id
                    JOIN Dormitories d ON b.DormId = d.Id
                    LEFT JOIN Students s ON r.Id = s.RoomId
                    GROUP BY r.Id
                    ORDER BY d.Name, b.Name, r.RoomNumber";

                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        occupancyList.Add(new {
                            Dormitory = reader.GetString(0),
                            Block = reader.GetString(1),
                            RoomNumber = reader.GetString(2),
                            Capacity = reader.GetInt32(3),
                            Occupants = reader.GetInt32(4),
                            Status = reader.GetInt32(4) >= reader.GetInt32(3) ? "Full" : (reader.GetInt32(4) == 0 ? "Empty" : "Available")
                        });
                    }
                }
            }
            return occupancyList;
        }

        // Report 3: Get assets by their status
        public List<Asset> GetAssetsByStatus(string status)
        {
            var assetRepo = new AssetRepository();
            return assetRepo.GetAll().Where(a => a.Status == status).ToList();
        }
    }
}