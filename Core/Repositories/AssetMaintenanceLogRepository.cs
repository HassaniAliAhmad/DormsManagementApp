using DormitoryManagement.Core.Database;
using DormitoryManagement.Core.Models;
using System.Collections.Generic;

namespace DormitoryManagement.Core.Repositories
{
    public class AssetMaintenanceLogRepository
    {
        private DbConnection _dbConnection;

        public AssetMaintenanceLogRepository()
        {
            _dbConnection = new DbConnection();
        }

        // Get all maintenance logs for a specific asset
        public List<AssetMaintenanceLog> GetByAssetId(int assetId)
        {
            var logs = new List<AssetMaintenanceLog>();
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, AssetId, RequestDate, Description, Status FROM AssetMaintenanceLogs WHERE AssetId = $assetId ORDER BY RequestDate DESC";
                command.Parameters.AddWithValue("$assetId", assetId);

                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        logs.Add(new AssetMaintenanceLog
                        {
                            Id = reader.GetInt32(0),
                            AssetId = reader.GetInt32(1),
                            RequestDate = reader.GetString(2),
                            Description = reader.GetString(3),
                            Status = reader.GetString(4)
                        });
                    }
                }
            }
            return logs;
        }

        // Add a new maintenance log entry
        public void Add(AssetMaintenanceLog log)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO AssetMaintenanceLogs (AssetId, RequestDate, Description, Status) VALUES ($assetId, $date, $desc, $status)";
                command.Parameters.AddWithValue("$assetId", log.AssetId);
                command.Parameters.AddWithValue("$date", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("$desc", log.Description);
                command.Parameters.AddWithValue("$status", log.Status);
                command.ExecuteNonQuery();
            }
        }
    }
}