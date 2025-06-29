using DormitoryManagement.Core.Database;
using DormitoryManagement.Core.Models;
using System.Collections.Generic;

namespace DormitoryManagement.Core.Repositories
{
    public class StudentRepository
    {
        private DbConnection _dbConnection;

        public StudentRepository()
        {
            _dbConnection = new DbConnection();
        }

        // Get all students with their location details
        public List<Student> GetAll()
        {
            var students = new List<Student>();
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                // This is a complex query to join all tables and get names
                command.CommandText = @"
                    SELECT 
                        p.Id, p.FirstName, p.LastName, p.NationalId, p.PhoneNumber, p.Address,
                        s.StudentId, s.RoomId,
                        r.RoomNumber, b.Name, d.Name
                    FROM Persons p
                    JOIN Students s ON p.Id = s.PersonId
                    LEFT JOIN Rooms r ON s.RoomId = r.Id
                    LEFT JOIN Blocks b ON r.BlockId = b.Id
                    LEFT JOIN Dormitories d ON b.DormId = d.Id
                    ORDER BY p.LastName, p.FirstName";
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            NationalId = reader.GetString(3),
                            PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                            StudentId = reader.GetString(6),
                            RoomId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                            RoomNumber = reader.IsDBNull(8) ? null : reader.GetString(8),
                            BlockName = reader.IsDBNull(9) ? null : reader.GetString(9),
                            DormitoryName = reader.IsDBNull(10) ? null : reader.GetString(10)
                        });
                    }
                }
            }
            return students;
        }

        // Add a new student
        public void Add(Student student)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                // We use a transaction because we are inserting into two tables
                using (var transaction = connection.BeginTransaction())
                {
                    // 1. Insert into Persons table
                    var personCommand = connection.CreateCommand();
                    personCommand.CommandText = "INSERT INTO Persons (FirstName, LastName, NationalId, PhoneNumber, Address) VALUES ($firstName, $lastName, $nationalId, $phone, $address)";
                    personCommand.Parameters.AddWithValue("$firstName", student.FirstName);
                    personCommand.Parameters.AddWithValue("$lastName", student.LastName);
                    personCommand.Parameters.AddWithValue("$nationalId", student.NationalId);
                    personCommand.Parameters.AddWithValue("$phone", student.PhoneNumber ?? (object)System.DBNull.Value);
                    personCommand.Parameters.AddWithValue("$address", student.Address ?? (object)System.DBNull.Value);
                    personCommand.ExecuteNonQuery();

                    // 2. Get the Id of the person we just inserted
                    var lastIdCommand = connection.CreateCommand();
                    lastIdCommand.CommandText = "SELECT last_insert_rowid()";
                    long personId = (long)lastIdCommand.ExecuteScalar();

                    // 3. Insert into Students table
                    var studentCommand = connection.CreateCommand();
                    studentCommand.CommandText = "INSERT INTO Students (PersonId, StudentId, RoomId) VALUES ($personId, $studentId, $roomId)";
                    studentCommand.Parameters.AddWithValue("$personId", personId);
                    studentCommand.Parameters.AddWithValue("$studentId", student.StudentId);
                    studentCommand.Parameters.AddWithValue("$roomId", student.RoomId ?? (object)System.DBNull.Value);
                    studentCommand.ExecuteNonQuery();

                    // If everything was successful, commit the transaction
                    transaction.Commit();
                }
            }
        }
        
        // Update an existing student's info and room assignment
        public void Update(Student student)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    // 1. Update Persons table
                    var personCommand = connection.CreateCommand();
                    personCommand.CommandText = "UPDATE Persons SET FirstName = $firstName, LastName = $lastName, NationalId = $nationalId, PhoneNumber = $phone, Address = $address WHERE Id = $id";
                    personCommand.Parameters.AddWithValue("$firstName", student.FirstName);
                    personCommand.Parameters.AddWithValue("$lastName", student.LastName);
                    personCommand.Parameters.AddWithValue("$nationalId", student.NationalId);
                    personCommand.Parameters.AddWithValue("$phone", student.PhoneNumber ?? (object)System.DBNull.Value);
                    personCommand.Parameters.AddWithValue("$address", student.Address ?? (object)System.DBNull.Value);
                    personCommand.Parameters.AddWithValue("$id", student.Id);
                    personCommand.ExecuteNonQuery();

                    // 2. Update Students table
                    var studentCommand = connection.CreateCommand();
                    studentCommand.CommandText = "UPDATE Students SET StudentId = $studentId, RoomId = $roomId WHERE PersonId = $id";
                    studentCommand.Parameters.AddWithValue("$studentId", student.StudentId);
                    studentCommand.Parameters.AddWithValue("$roomId", student.RoomId ?? (object)System.DBNull.Value);
                    studentCommand.Parameters.AddWithValue("$id", student.Id);
                    studentCommand.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
        }

        // Delete a student
        public void Delete(int personId)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    // Must delete from child table (Students) first
                    var studentCommand = connection.CreateCommand();
                    studentCommand.CommandText = "DELETE FROM Students WHERE PersonId = $id";
                    studentCommand.Parameters.AddWithValue("$id", personId);
                    studentCommand.ExecuteNonQuery();

                    // Then delete from parent table (Persons)
                    var personCommand = connection.CreateCommand();
                    personCommand.CommandText = "DELETE FROM Persons WHERE Id = $id";
                    personCommand.Parameters.AddWithValue("$id", personId);
                    personCommand.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
        }
        public int GetStudentCountByRoomId(int roomId)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Students WHERE RoomId = $roomId";
                command.Parameters.AddWithValue("$roomId", roomId);
                
                var result = command.ExecuteScalar();
                return result != null ? System.Convert.ToInt32(result) : 0;
            }
        }
    
    }
}