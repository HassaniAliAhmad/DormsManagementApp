using DormitoryManagement.Core.Database;
using DormitoryManagement.Core.Models;
using System.Collections.Generic;

namespace DormitoryManagement.Core.Repositories
{
    public class RoomRepository
    {
        private DbConnection _dbConnection;

        public RoomRepository()
        {
            _dbConnection = new DbConnection();
        }

        // Get all rooms for a SPECIFIC block
        public List<Room> GetByBlockId(int blockId)
        {
            var rooms = new List<Room>();
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT r.Id, r.RoomNumber, r.Floor, r.Capacity, r.BlockId, b.Name
                    FROM Rooms r
                    JOIN Blocks b ON r.BlockId = b.Id
                    WHERE r.BlockId = $blockId
                    ORDER BY r.Floor, r.RoomNumber";
                command.Parameters.AddWithValue("$blockId", blockId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rooms.Add(new Room
                        {
                            Id = reader.GetInt32(0),
                            RoomNumber = reader.GetString(1),
                            Floor = reader.GetInt32(2),
                            Capacity = reader.GetInt32(3),
                            BlockId = reader.GetInt32(4),
                            BlockName = reader.GetString(5)
                        });
                    }
                }
            }
            return rooms;
        }

        // Add a new room
        public void Add(Room room)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Rooms (RoomNumber, Floor, Capacity, BlockId) VALUES ($roomNumber, $floor, $capacity, $blockId)";
                command.Parameters.AddWithValue("$roomNumber", room.RoomNumber);
                command.Parameters.AddWithValue("$floor", room.Floor);
                command.Parameters.AddWithValue("$capacity", room.Capacity);
                command.Parameters.AddWithValue("$blockId", room.BlockId);
                command.ExecuteNonQuery();
            }
        }

        // Delete a room
        public void Delete(int roomId)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Rooms WHERE Id = $id";
                command.Parameters.AddWithValue("$id", roomId);
                command.ExecuteNonQuery();
            }
        }
         public Room? GetById(int roomId)
        {
            Room? room = null;
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, RoomNumber, Floor, Capacity, BlockId FROM Rooms WHERE Id = $id";
                command.Parameters.AddWithValue("$id", roomId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        room = new Room
                        {
                            Id = reader.GetInt32(0),
                            RoomNumber = reader.GetString(1),
                            Floor = reader.GetInt32(2),
                            Capacity = reader.GetInt32(3),
                            BlockId = reader.GetInt32(4)
                        };
                    }
                }
            }
            return room;
        }
    
    }
}