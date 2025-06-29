using DormitoryManagement.Core.Database;
using DormitoryManagement.Core.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace DormitoryManagement.Core.Repositories
{
    public class DormitoryRepository
    {
        private DbConnection _dbConnection;

        public DormitoryRepository()
        {
            _dbConnection = new DbConnection();
        }

        public List<Dormitory> GetAll()
        {
            var dormitories = new List<Dormitory>();
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, Address, TotalCapacity, DormOfficialId FROM Dormitories"; // Added DormOfficialId

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dormitories.Add(new Dormitory
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Address = reader.IsDBNull(2) ? null : reader.GetString(2),
                            TotalCapacity = reader.GetInt32(3),
                            DormOfficialId = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4) // Get the official's ID
                        });
                    }
                }
            }
            return dormitories;
        }

        public void Add(Dormitory dormitory)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Dormitories (Name, Address, TotalCapacity, DormOfficialId) VALUES ($name, $address, $capacity, $officialId)";
                command.Parameters.AddWithValue("$name", dormitory.Name);
                command.Parameters.AddWithValue("$address", dormitory.Address ?? (object)System.DBNull.Value);
                command.Parameters.AddWithValue("$capacity", dormitory.TotalCapacity);
                command.Parameters.AddWithValue("$officialId", dormitory.DormOfficialId ?? (object)System.DBNull.Value); // Add official
                command.ExecuteNonQuery();
            }
        }
        
        // --- NEW METHOD ---
        public void Update(Dormitory dormitory)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Dormitories SET
                        Name = $name,
                        Address = $address,
                        TotalCapacity = $capacity,
                        DormOfficialId = $officialId
                    WHERE Id = $id";
                command.Parameters.AddWithValue("$name", dormitory.Name);
                command.Parameters.AddWithValue("$address", dormitory.Address ?? (object)System.DBNull.Value);
                command.Parameters.AddWithValue("$capacity", dormitory.TotalCapacity);
                command.Parameters.AddWithValue("$officialId", dormitory.DormOfficialId ?? (object)System.DBNull.Value);
                command.Parameters.AddWithValue("$id", dormitory.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int dormitoryId)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Dormitories WHERE Id = $id";
                command.Parameters.AddWithValue("$id", dormitoryId);
                command.ExecuteNonQuery();
            }
        }
    }
}