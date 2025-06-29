using DormitoryManagement.Core.Database;
using DormitoryManagement.Core.Models;
using System.Collections.Generic;

namespace DormitoryManagement.Core.Repositories
{
    public class AssetRepository
    {
        private DbConnection _dbConnection;

        public AssetRepository()
        {
            _dbConnection = new DbConnection();
        }

        public List<Asset> GetAll()
        {
            var assets = new List<Asset>();
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT
                        a.Id, a.AssetNumber, a.Type, a.PartNumber, a.Status,
                        a.RoomId, a.OwningStudentId,
                        r.RoomNumber,
                        b.Name as BlockName,
                        d.Name as DormitoryName,
                        p.FirstName || ' ' || p.LastName as StudentFullName
                    FROM Assets a
                    LEFT JOIN Rooms r ON a.RoomId = r.Id
                    LEFT JOIN Blocks b ON r.BlockId = b.Id
                    LEFT JOIN Dormitories d ON b.DormId = d.Id
                    LEFT JOIN Persons p ON a.OwningStudentId = p.Id
                    ORDER BY a.Type, a.AssetNumber";
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        assets.Add(new Asset
                        {
                            Id = reader.GetInt32(0),
                            AssetNumber = reader.GetString(1),
                            Type = reader.GetString(2),
                            PartNumber = reader.GetString(3),
                            Status = reader.GetString(4),
                            RoomId = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                            OwningStudentId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            RoomNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                            BlockName = reader.IsDBNull(8) ? null : reader.GetString(8),
                            DormitoryName = reader.IsDBNull(9) ? null : reader.GetString(9),
                            StudentFullName = reader.IsDBNull(10) ? null : reader.GetString(10)
                        });
                    }
                }
            }
            return assets;
        }

        public void Add(Asset asset)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Assets (AssetNumber, Type, PartNumber, Status, RoomId, OwningStudentId) VALUES ($assetNum, $type, $partNum, $status, $roomId, $studentId)";
                command.Parameters.AddWithValue("$assetNum", asset.AssetNumber);
                command.Parameters.AddWithValue("$type", asset.Type);
                command.Parameters.AddWithValue("$partNum", asset.PartNumber);
                command.Parameters.AddWithValue("$status", asset.Status);
                command.Parameters.AddWithValue("$roomId", asset.RoomId ?? (object)System.DBNull.Value);
                command.Parameters.AddWithValue("$studentId", asset.OwningStudentId ?? (object)System.DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Asset asset)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Assets SET
                        AssetNumber = $assetNum,
                        Type = $type,
                        PartNumber = $partNum,
                        Status = $status,
                        RoomId = $roomId,
                        OwningStudentId = $studentId
                    WHERE Id = $id";
                command.Parameters.AddWithValue("$assetNum", asset.AssetNumber);
                command.Parameters.AddWithValue("$type", asset.Type);
                command.Parameters.AddWithValue("$partNum", asset.PartNumber);
                command.Parameters.AddWithValue("$status", asset.Status);
                command.Parameters.AddWithValue("$roomId", asset.RoomId ?? (object)System.DBNull.Value);
                command.Parameters.AddWithValue("$studentId", asset.OwningStudentId ?? (object)System.DBNull.Value);
                command.Parameters.AddWithValue("$id", asset.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int assetId)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Assets WHERE Id = $id";
                command.Parameters.AddWithValue("$id", assetId);
                command.ExecuteNonQuery();
            }
        }
    }
}