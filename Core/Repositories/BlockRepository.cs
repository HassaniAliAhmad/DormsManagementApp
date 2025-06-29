using DormitoryManagement.Core.Database;
using DormitoryManagement.Core.Models;
using System.Collections.Generic;

namespace DormitoryManagement.Core.Repositories
{
    public class BlockRepository
    {
        private DbConnection _dbConnection;

        public BlockRepository()
        {
            _dbConnection = new DbConnection();
        }

        // GetByDormitoryId is updated to also fetch the BlockOfficialId
        public List<Block> GetByDormitoryId(int dormitoryId)
        {
            var blocks = new List<Block>();
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT b.Id, b.Name, b.DormId, d.Name, b.BlockOfficialId 
                    FROM Blocks b
                    JOIN Dormitories d ON b.DormId = d.Id
                    WHERE b.DormId = $dormId";
                command.Parameters.AddWithValue("$dormId", dormitoryId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        blocks.Add(new Block
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            DormId = reader.GetInt32(2),
                            DormitoryName = reader.GetString(3),
                            BlockOfficialId = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4)
                        });
                    }
                }
            }
            return blocks;
        }

        // Add method is updated to include BlockOfficialId
        public void Add(Block block)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Blocks (Name, DormId, BlockOfficialId) VALUES ($name, $dormId, $officialId)";
                command.Parameters.AddWithValue("$name", block.Name);
                command.Parameters.AddWithValue("$dormId", block.DormId);
                command.Parameters.AddWithValue("$officialId", block.BlockOfficialId ?? (object)System.DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        // New Update method is added
        public void Update(Block block)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Blocks SET
                        Name = $name,
                        BlockOfficialId = $officialId
                    WHERE Id = $id";
                command.Parameters.AddWithValue("$name", block.Name);
                command.Parameters.AddWithValue("$officialId", block.BlockOfficialId ?? (object)System.DBNull.Value);
                command.Parameters.AddWithValue("$id", block.Id);
                command.ExecuteNonQuery();
            }
        }

        // Delete method is unchanged
        public void Delete(int blockId)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Blocks WHERE Id = $id";
                command.Parameters.AddWithValue("$id", blockId);
                command.ExecuteNonQuery();
            }
        }
    }
}